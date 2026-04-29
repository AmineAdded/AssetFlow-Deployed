// src/Backend/AssetFlow.Infrastructure/Services/AgentService.cs
using AssetFlow.Application.DTOs;
using AssetFlow.Application.DTOs.AgentDtos;
using AssetFlow.Application.Interfaces;
using AssetFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AssetFlow.Infrastructure.Services
{
    public class AgentService : IAgentService
    {
        private readonly IOrchestratorAgentService _orchestrator;
        private readonly IWebSearchAgentService    _webSearch;
        private readonly IDatabaseAgentService     _dbAgent;
        private readonly ICommandeService          _commandeService;
        private readonly IMaterielService          _materielService;
        private readonly AppDbContext              _db;

        public AgentService(
            IOrchestratorAgentService orchestrator,
            IWebSearchAgentService    webSearch,
            IDatabaseAgentService     dbAgent,
            ICommandeService          commandeService,
            IMaterielService          materielService,
            AppDbContext              db)
        {
            _orchestrator    = orchestrator;
            _webSearch       = webSearch;
            _dbAgent         = dbAgent;
            _commandeService = commandeService;
            _materielService = materielService;
            _db              = db;
        }

        // ── Traitement principal d'un message ─────────────────────────────
        public async Task<AgentChatResponse> ProcessMessageAsync(AgentChatRequest request)
        {
            // L'historique est passé à tous les agents pour maintenir le contexte
            var history   = request.History;
            var agentType = await _orchestrator.DetermineAgentAsync(request.Message, history);
            var response  = new AgentChatResponse { AgentUsed = agentType };

            if (agentType == "web")
            {
                response.Message   = await _webSearch.SearchAsync(request.Message, history);
                response.AgentUsed = "web";
            }
            else if (agentType == "db")
            {
                // Détecter si l'utilisateur essaie de créer un nouveau matériel (action non autorisée)
                var msg = request.Message.ToLower();
                var isCreationAttempt = (msg.Contains("ajoute") || msg.Contains("crée") || msg.Contains("créer") || msg.Contains("nouveau") || msg.Contains("nouvelle") || msg.Contains("add") || msg.Contains("insert"))
                    && (msg.Contains("matériel") || msg.Contains("materiel") || msg.Contains("équipement") || msg.Contains("equipement"));

                if (isCreationAttempt)
                {
                    response.Message   = "❌ Je ne suis pas autorisé à créer de nouveaux matériels. Seules les commandes sur des matériels existants sont permises. Si vous souhaitez commander un matériel existant, précisez sa référence.";
                    response.AgentUsed = "db";
                    return response;
                }

                response.Message   = await _dbAgent.QueryAsync(request.Message, history);
                response.AgentUsed = "db";
            }
            else if (agentType.StartsWith("action_"))
            {
                var action = await _orchestrator.ExtractActionAsync(request.Message, history);
                response.AgentUsed = "action";
                response.Action    = action;

                // ── Si add_materiel : vérifier si référence existe déjà ──
                if (action?.Type == "add_materiel" && action.MaterielProposal != null)
                {
                    var ref_ = action.MaterielProposal.Reference?.Trim();
                    if (!string.IsNullOrWhiteSpace(ref_))
                    {
                        var existant = await _db.Materiels
                            .FirstOrDefaultAsync(m => m.Reference.ToLower() == ref_.ToLower());

                        if (existant != null)
                        {
                            // Matériel existant → autoriser la commande
                            action.MaterielProposal.Reference     = existant.Reference;
                            action.MaterielProposal.Designation   = existant.Designation;
                            action.MaterielProposal.Description   = existant.Description;
                            action.MaterielProposal.Categorie     = existant.Categorie;
                            action.MaterielProposal.QuantiteStock = existant.QuantiteStock;
                            action.MaterielProposal.QuantiteMin   = existant.QuantiteMin;
                            action.MaterielProposal.Unite         = existant.Unite;
                            action.MaterielProposal.Emplacement   = existant.Emplacement;
                            action.Label = $"exists:{existant.Id}";
                        }
                        else
                        {
                            // ← NOUVEAU : bloquer la création d'un nouveau matériel
                            response.AgentUsed = "db";
                            response.Action    = null;
                            response.Message   = "❌ La création de nouveaux matériels n'est pas autorisée. Seuls les matériels existants peuvent recevoir une commande. Vérifiez la référence ou contactez un administrateur.";
                            return response;
                        }
                    }
                    else
                    {
                        // Référence vide → on ne peut pas vérifier
                        response.AgentUsed = "db";
                        response.Action    = null;
                        response.Message   = "❌ Impossible de créer un matériel sans référence. Précisez la référence du matériel existant.";
                        return response;
                    }
                }

                response.Message = action?.Type switch
                {
                    "add_materiel"  => action.Label.StartsWith("exists:")
                        ? $"Le matériel **{action.MaterielProposal?.Designation}** existe déjà. Voici le formulaire pour ajouter une commande à ce matériel."
                        : $"J'ai préparé une proposition pour créer le matériel **{action.MaterielProposal?.Designation}**. Veuillez vérifier et approuver les informations ci-dessous.",
                    "add_commande"  => $"J'ai préparé une proposition de commande **{action.CommandeProposal?.NumeroCommande}**. Veuillez vérifier et approuver les informations.",
                    "add_article"   => $"J'ai préparé une proposition pour ajouter un article. Veuillez vérifier les informations.",
                    _               => "Voici la proposition générée. Veuillez l'approuver ou la modifier."
                };
            }
            else
            {
                response.Message   = await _dbAgent.QueryAsync(request.Message, history);
                response.AgentUsed = "db";
            }

            return response;
        }

        // ── Alertes initiales à l'ouverture du chat ───────────────────────
        public async Task<AgentChatResponse> GetInitialAlertsAsync()
        {
            var alertes = await _dbAgent.GetStockAlertsAsync();
            var response = new AgentChatResponse
            {
                AgentUsed = "db",
                Alertes   = alertes
            };

            if (alertes.Count == 0)
            {
                response.Message = "✅ Tous les niveaux de stock sont OK. Aucune alerte détectée.";
                return response;
            }

            var tasks = alertes.Select(async a =>
            {
                a.Proposition = await _orchestrator.GenerateMaterielProposalAsync(a);
            });
            await Task.WhenAll(tasks);

            response.Alertes = alertes;
            response.Message = $"⚠️ **{alertes.Count} alerte(s) de stock détectée(s)** :\n\n" +
                string.Join("\n", alertes.Select(a =>
                    $"• **{a.Designation}** ({a.Reference}) : stock actuel **{a.QuantiteStock}** / minimum **{a.QuantiteMin}**"));

            return response;
        }

        // ── Approbation d'une action ──────────────────────────────────────
        public async Task<AgentApprovalResponse> ApproveActionAsync(AgentApprovalRequest request)
        {
            if (!request.Approved)
                return new AgentApprovalResponse { Succes = false, Message = "Action annulée." };

            try
            {
                switch (request.ActionType)
                {
                    case "add_materiel":
                    {
                        if (request.MaterielProposal == null)
                            return Fail("Données matériel manquantes.");

                        var p = request.MaterielProposal;

                        var existant = await _db.Materiels
                            .FirstOrDefaultAsync(m => m.Reference.ToLower() == p.Reference.Trim().ToLower());

                        // ── NOUVEAU : bloquer la création, seuls les matériels existants sont autorisés ──
                        if (existant == null)
                            return Fail("❌ Création de nouveaux matériels non autorisée. Le matériel doit déjà exister en base.");

                        int materielId = existant.Id;

                        if (p.Commande != null && !string.IsNullOrWhiteSpace(p.Commande.NumeroCommande))
                        {
                            var doublonCmd = await _db.Commandes
                                .FirstOrDefaultAsync(c => c.NumeroCommande.ToLower() == p.Commande.NumeroCommande.Trim().ToLower());

                            if (doublonCmd != null)
                                return new AgentApprovalResponse
                                {
                                    Succes  = false,
                                    Message = $"duplicate_commande:{p.Commande.NumeroCommande}"
                                };

                            var fournisseurId = p.Commande.FournisseurId;
                            if (fournisseurId == 0 && !string.IsNullOrWhiteSpace(p.Commande.NomFournisseur))
                            {
                                var f = await _db.Fournisseurs.FirstOrDefaultAsync(x =>
                                    x.Nom.ToLower() == p.Commande.NomFournisseur.Trim().ToLower());
                                if (f == null)
                                {
                                    var newF = new Domain.Entities.Fournisseur { Nom = p.Commande.NomFournisseur.Trim() };
                                    _db.Fournisseurs.Add(newF);
                                    await _db.SaveChangesAsync();
                                    fournisseurId = newF.IdFournisseur;
                                }
                                else fournisseurId = f.IdFournisseur;
                            }

                            await _commandeService.CreerAsync(new CreerCommandeDto
                            {
                                Utilisateur         = request.Utilisateur,
                                NumeroCommande      = p.Commande.NumeroCommande,
                                MaterielId          = materielId,
                                FournisseurId       = fournisseurId,
                                NomFournisseurLibre = p.Commande.NomFournisseur,
                                QuantiteAchetee     = p.Commande.QuantiteAchetee,
                                DateAchat           = p.Commande.DateAchat,
                                DateLivraison       = p.Commande.DateLivraison,
                                DateFinGarantie     = p.Commande.DateFinGarantie,
                                NumerosSerie        = p.Commande.NumerosSerie
                            });
                        }

                        return new AgentApprovalResponse
                        {
                            Succes  = true,
                            Message = $"✅ Commande ajoutée au matériel existant **{existant.Designation}** !",
                            Id      = materielId
                        };
                    }

                    case "add_commande":
                    {
                        if (request.CommandeProposal == null)
                            return Fail("Données commande manquantes.");

                        var p = request.CommandeProposal;

                        var doublon = await _db.Commandes
                            .FirstOrDefaultAsync(c => c.NumeroCommande.ToLower() == p.NumeroCommande.Trim().ToLower());

                        if (doublon != null)
                            return new AgentApprovalResponse
                            {
                                Succes  = false,
                                Message = $"duplicate_commande:{p.NumeroCommande}"
                            };

                        var fournisseurId = p.FournisseurId;
                        if (fournisseurId == 0 && !string.IsNullOrWhiteSpace(p.NomFournisseur))
                        {
                            var f = await _db.Fournisseurs.FirstOrDefaultAsync(x =>
                                x.Nom.ToLower() == p.NomFournisseur.Trim().ToLower());
                            if (f == null)
                            {
                                var newF = new Domain.Entities.Fournisseur { Nom = p.NomFournisseur.Trim() };
                                _db.Fournisseurs.Add(newF);
                                await _db.SaveChangesAsync();
                                fournisseurId = newF.IdFournisseur;
                            }
                            else fournisseurId = f.IdFournisseur;
                        }

                        var result = await _commandeService.CreerAsync(new CreerCommandeDto
                        {
                            Utilisateur     = request.Utilisateur,
                            NumeroCommande  = p.NumeroCommande,
                            MaterielId      = p.MaterielId,
                            FournisseurId   = fournisseurId,
                            QuantiteAchetee = p.QuantiteAchetee,
                            DateAchat       = p.DateAchat,
                            DateLivraison   = p.DateLivraison,
                            DateFinGarantie = p.DateFinGarantie,
                            NumerosSerie    = p.NumerosSerie
                        });

                        if (!result.Succes) return Fail(result.Message);
                        return new AgentApprovalResponse
                        {
                            Succes  = true,
                            Message = $"✅ Commande **{p.NumeroCommande}** créée avec succès !",
                            Id      = result.IdCommande
                        };
                    }

                    case "add_article":
                    {
                        if (request.ArticleProposal == null)
                            return Fail("Données article manquantes.");

                        var p = request.ArticleProposal;
                        var commande = await _db.Commandes
                            .Include(c => c.Materiel)
                            .FirstOrDefaultAsync(c => c.Id == p.CommandeId);

                        if (commande == null)
                            return Fail("Commande introuvable.");

                        var article = new Domain.Entities.ArticleIndividuel
                        {
                            NumeroSerie = string.IsNullOrWhiteSpace(p.NumeroSerie) ? null : p.NumeroSerie.Trim(),
                            Statut      = Domain.Entities.StatutArticle.Disponible,
                            MaterielId  = p.MaterielId > 0 ? p.MaterielId : commande.MaterielId,
                            CommandeId  = p.CommandeId
                        };
                        _db.ArticlesIndividuels.Add(article);

                        if (commande.Materiel != null) commande.Materiel.QuantiteStock++;

                        _db.ArticleHistoriques.Add(new Domain.Entities.ArticleHistorique
                        {
                            ArticleId     = article.Id,
                            TypeEvenement = Domain.Entities.TypeEvenementArticle.Acquisition,
                            DateEvenement = DateTime.UtcNow,
                            Description   = $"Ajout via agent IA — commande {commande.NumeroCommande}"
                        });

                        await _db.SaveChangesAsync();
                        return new AgentApprovalResponse
                        {
                            Succes  = true,
                            Message = $"✅ Article ajouté avec succès à la commande {commande.NumeroCommande} !",
                            Id      = article.Id
                        };
                    }

                    default:
                        return Fail($"Type d'action inconnu : {request.ActionType}");
                }
            }
            catch (Exception ex)
            {
                return Fail($"Erreur : {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        private static AgentApprovalResponse Fail(string msg)
            => new() { Succes = false, Message = msg };
    }
}