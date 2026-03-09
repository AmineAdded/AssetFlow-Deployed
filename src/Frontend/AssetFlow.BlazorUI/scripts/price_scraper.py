#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
Script de Web Scraping MyTek (par catégorie) + Tunisianet + SpaceNet (Selenium)
Auteur: STAMBOULI Nada
Projet: AssetFlow — Gestion de Parc Informatique
"""

import sys
import json
import time
import os
from datetime import datetime

from selenium import webdriver
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.common.by import By
from webdriver_manager.chrome import ChromeDriverManager


# =============================================================================
# CHARGEMENT DU MAPPING CATÉGORIES MYTEK DEPUIS LE FICHIER JSON
# =============================================================================

def charger_categories_mytek() -> dict:
    json_path = os.path.join(os.path.dirname(__file__), "mytek_categories.json")
    try:
        with open(json_path, "r", encoding="utf-8") as f:
            data = json.load(f)
        plat = {}
        for section in data.values():
            plat.update(section)
        print(f"[MyTek] ✅ {len(plat)} entrées de catégories chargées")
        return plat
    except FileNotFoundError:
        print(f"[MyTek] ❌ mytek_categories.json introuvable : {json_path}")
        return {}
    except json.JSONDecodeError as e:
        print(f"[MyTek] ❌ Erreur JSON : {e}")
        return {}


MYTEK_CATEGORIES = charger_categories_mytek()


def trouver_url_categorie_mytek(query: str) -> str | None:
    q = query.lower().strip()
    # 1. Correspondance exacte
    if q in MYTEK_CATEGORIES:
        return MYTEK_CATEGORIES[q]
    # 2. La clé est contenue dans la query (ex: "souris logitech" contient "souris")
    for cle, url in MYTEK_CATEGORIES.items():
        if cle in q:
            return url
    # 3. La query est contenue dans la clé
    for cle, url in MYTEK_CATEGORIES.items():
        if q in cle:
            return url
    return None


# =============================================================================
# CRÉATION DU DRIVER
# =============================================================================

def creer_driver():
    options = webdriver.ChromeOptions()
    options.add_argument("--headless")
    options.add_argument("--window-size=1920,1080")
    options.add_argument("--no-sandbox")
    options.add_argument("--disable-dev-shm-usage")
    return webdriver.Chrome(
        service=Service(ChromeDriverManager().install()),
        options=options
    )


# =============================================================================
# SCRAPING MYTEK
# Sélecteur unifié : div.product-container (même HTML pour catégorie ET recherche)
# =============================================================================

def scraper_mytek(nom_article: str, driver) -> list:
    url_categorie = trouver_url_categorie_mytek(nom_article)

    if url_categorie:
        print(f"\n[MyTek] ✅ Catégorie trouvée → {url_categorie}")
        url           = url_categorie
        par_categorie = True
    else:
        url = (
            "https://www.mytek.tn/myteksearch/index/productsearch/?q="
            + nom_article.replace(" ", "+")
        )
        par_categorie = False
        print(f"\n[MyTek] ⚠ Fallback recherche → {url}")

    driver.get(url)
    time.sleep(3)

    # ── Sélecteur unifié ──────────────────────────────────────────────────────
    # Les pages de catégorie ET de recherche utilisent le même div.product-container
    produits = driver.find_elements(By.CSS_SELECTOR, "div.product-container")
    print(f"[MyTek] {len(produits)} produit(s) trouvé(s)")

    if not produits:
        return []

    resultats = []
    # Pour les catégories larges (ex: "pc portable"), on ne filtre pas par mot-clé.
    # Pour les recherches précises (ex: "macbook pro 14"), on filtre.
    mots_significatifs = [m for m in nom_article.lower().split() if len(m) > 3]

    for produit in produits:

        # NOM
        try:
            nom = produit.find_element(
                By.CSS_SELECTOR, "a.product-item-link"
            ).text.strip()
        except:
            nom = nom_article

        # Filtre mot-clé : seulement si recherche précise (2+ mots significatifs)
        # Ex: "macbook" dans catégorie Mac → pas de filtre (tout est pertinent)
        # Ex: "macbook pro 14" → filtre sur "macbook", "pro"
        if par_categorie and len(mots_significatifs) >= 2:
            nom_lower = nom.lower()
            if not any(m in nom_lower for m in mots_significatifs):
                continue

        # PRIX — span.final-price (identique catégorie et recherche)
        try:
            prix_txt = produit.find_element(
                By.CSS_SELECTOR, "span.final-price"
            ).text
            # Nettoyage complet : tous types d'espaces + séparateurs
            prix_clean = (
                prix_txt
                .replace("DT", "")
                .replace("TND", "")
                .replace("\xa0", "")   # espace insécable
                .replace("\u202f", "") # espace fine insécable
                .replace("\u2009", "") # espace fine
                .replace(" ", "")      # espace normal
                .replace("\t", "")
                .strip()
            )
            # MyTek utilise la virgule comme séparateur décimal : "2499,000" → "2499.000"
            prix_clean = prix_clean.replace(",", ".")
            # Si plusieurs points (ex: "2.499.000"), garder uniquement le dernier
            parties = prix_clean.split(".")
            if len(parties) > 2:
                prix_clean = "".join(parties[:-1]) + "." + parties[-1]
            prix = float(prix_clean)
        except Exception as e:
            print(f"  [MyTek] ⚠ Prix introuvable pour : {nom[:50]} — '{prix_txt if 'prix_txt' in dir() else '?'}' ({e})")
            continue

        # STOCK
        stock = "Non indiqué"
        try:
            classes = produit.find_element(
                By.CSS_SELECTOR, "div.stock"
            ).get_attribute("class")
            if "availables"     in classes: stock = "En stock"
            elif "incoming"     in classes: stock = "En arrivage"
            elif "out-of-stock" in classes: stock = "Épuisé"
            elif "special-order" in classes: stock = "Sur commande"
            else:                           stock = "État inconnu"
        except:
            pass

        # LIEN
        try:
            lien = produit.find_element(
                By.CSS_SELECTOR, "a.product-item-link"
            ).get_attribute("href")
        except:
            lien = url

        resultats.append({
            "site":          "MyTek",
            "nom_produit":   nom,
            "prix":          round(prix, 3),
            "devise":        "TND",
            "stock":         stock,
            "url":           lien,
            "date_scraping": datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        })

    return resultats


# =============================================================================
# SCRAPING TUNISIANET (inchangé)
# =============================================================================

def scraper_tunisianet(nom_article: str, driver) -> list:
    url = (
        "https://www.tunisianet.com.tn/recherche?controller=search"
        "&orderby=price&orderway=asc&s="
        + nom_article.replace(" ", "+")
        + "&submit_search="
    )
    print(f"\n[Tunisianet] URL : {url}")
    driver.get(url)
    time.sleep(3)

    produits = driver.find_elements(By.CSS_SELECTOR, "article.product-miniature")
    print(f"[Tunisianet] {len(produits)} produit(s) trouvé(s)")
    if not produits:
        return []

    resultats = []
    for produit in produits:
        try:
            lien_el = produit.find_element(By.CSS_SELECTOR, "h2.product-title a")
            nom     = lien_el.text.strip()
            lien    = lien_el.get_attribute("href")
        except:
            nom  = nom_article
            lien = url

        try:
            prix_els = produit.find_elements(By.CSS_SELECTOR, "span[itemprop='price']")
            if not prix_els:
                prix_els = produit.find_elements(By.CSS_SELECTOR, "span.price")
            prix_txt = ""
            for el in prix_els:
                txt = el.text.strip()
                if txt and ("DT" in txt or any(c.isdigit() for c in txt)):
                    prix_txt = txt
                    break
            if not prix_txt:
                for el in prix_els:
                    content = el.get_attribute("content")
                    if content:
                        prix_txt = content
                        break
            prix = float(
                prix_txt.replace("DT", "").replace("TND", "")
                        .replace(",", ".").replace(" ", "").replace("\xa0", "").strip()
            )
        except Exception as e:
            print(f"  [Tunisianet] ⚠ Prix introuvable pour : {nom[:50]} ({e})")
            continue

        stock = "Non indiqué"
        try:
            spans = produit.find_elements(By.CSS_SELECTOR, "div#stock_availability span")
            for span in spans:
                txt = span.text.strip()
                if txt:
                    stock = txt
                    break
        except:
            pass

        resultats.append({
            "site":          "Tunisianet",
            "nom_produit":   nom,
            "prix":          round(prix, 3),
            "devise":        "TND",
            "stock":         stock,
            "url":           lien,
            "date_scraping": datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        })

    return resultats


# =============================================================================
# SCRAPING SPACENET (inchangé)
# =============================================================================

def scraper_spacenet(nom_article: str, driver) -> list:
    url = (
        "https://spacenet.tn/recherche?controller=search"
        "&orderby=position&orderway=desc&search_query="
        + nom_article.replace(" ", "+")
        + "&submit_search="
    )
    print(f"\n[Spacenet] URL : {url}")
    driver.get(url)
    time.sleep(3)

    produits = driver.find_elements(
        By.CSS_SELECTOR, "div.field-product-item.product-miniature"
    )
    print(f"[Spacenet] {len(produits)} produit(s) trouvé(s)")
    if not produits:
        return []

    resultats = []
    for produit in produits:
        try:
            lien_el = produit.find_element(By.CSS_SELECTOR, "h2.product_name a")
            nom     = lien_el.text.strip()
            lien    = lien_el.get_attribute("href")
        except:
            nom  = nom_article
            lien = url

        try:
            prix_els = produit.find_elements(By.CSS_SELECTOR, "span.price")
            prix_txt = ""
            for el in prix_els:
                txt = el.text.strip()
                if txt and any(c.isdigit() for c in txt):
                    prix_txt = txt
                    break
            prix = float(
                prix_txt.replace("DT", "").replace("TND", "")
                        .replace(",", ".").replace("\xa0", "").replace(" ", "").strip()
            )
        except Exception as e:
            print(f"  [Spacenet] ⚠ Prix introuvable pour : {nom[:50]} ({e})")
            continue

        stock = "Non indiqué"
        try:
            label = produit.find_element(By.CSS_SELECTOR, "label.label-available")
            stock = label.text.strip()
        except:
            pass

        resultats.append({
            "site":          "Spacenet",
            "nom_produit":   nom,
            "prix":          round(prix, 3),
            "devise":        "TND",
            "stock":         stock,
            "url":           lien,
            "date_scraping": datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        })

    return resultats


# =============================================================================
# FONCTION PRINCIPALE
# =============================================================================

def scraper_prix(nom_article: str) -> dict:
    driver         = creer_driver()
    tous_resultats = []

    try:
        res_mytek = scraper_mytek(nom_article, driver)
        tous_resultats.extend(res_mytek)
        print(f"[MyTek] {len(res_mytek)} résultat(s) récupéré(s)")

        res_tunisianet = scraper_tunisianet(nom_article, driver)
        tous_resultats.extend(res_tunisianet)
        print(f"[Tunisianet] {len(res_tunisianet)} résultat(s) récupéré(s)")

        res_spacenet = scraper_spacenet(nom_article, driver)
        tous_resultats.extend(res_spacenet)
        print(f"[Spacenet] {len(res_spacenet)} résultat(s) récupéré(s)")

    finally:
        driver.quit()

    if tous_resultats:
        meilleur = min(tous_resultats, key=lambda x: x["prix"])
        return {
            "succes":           True,
            "article":          nom_article,
            "date_recherche":   datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
            "nombre_resultats": len(tous_resultats),
            "resultats":        tous_resultats,
            "meilleur_prix":    meilleur,
            "recommandation": {
                "site":    meilleur["site"],
                "prix":    meilleur["prix"],
                "url":     meilleur["url"],
                "message": f"Meilleur prix trouvé sur {meilleur['site']}"
            }
        }

    return {
        "succes":           False,
        "article":          nom_article,
        "date_recherche":   datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
        "nombre_resultats": 0,
        "resultats":        [],
        "meilleur_prix":    None,
        "recommandation": {
            "site": None, "prix": None, "url": None,
            "message": f"Aucun résultat trouvé pour '{nom_article}'"
        }
    }


# =============================================================================
# AFFICHAGE CONSOLE
# =============================================================================

def afficher_resultats(reponse: dict):
    print("\n" + "=" * 60)
    print("           RÉSULTATS DU SCRAPING")
    print("=" * 60)

    if not reponse["succes"]:
        print("❌ Aucun résultat trouvé")
        return

    print(f"Article      : {reponse['article']}")
    print(f"Nb résultats : {reponse['nombre_resultats']}")
    print()

    sites = {}
    for r in reponse["resultats"]:
        sites.setdefault(r["site"], []).append(r)

    for site, produits in sites.items():
        print(f"── {site} ({len(produits)} produit(s)) ──")
        for i, r in enumerate(produits, 1):
            print(f"  [{i}] {r['nom_produit'][:60]}")
            print(f"       Prix  : {r['prix']} TND")
            print(f"       Stock : {r['stock']}")
        print()

    best = reponse["meilleur_prix"]
    print("=" * 60)
    print("🏆 MEILLEUR PRIX")
    print(f"   Site    : {best['site']}")
    print(f"   Produit : {best['nom_produit'][:60]}")
    print(f"   Prix    : {best['prix']} TND")
    print("=" * 60)


# =============================================================================
# MAIN
# =============================================================================

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python price_scraper.py <nom_article>")
        print("Exemples:")
        print("  python price_scraper.py macbook")
        print("  python price_scraper.py souris")
        print("  python price_scraper.py ssd")
        sys.exit(1)

    nom_article = " ".join(sys.argv[1:])
    print(f"\n🔍 Recherche : '{nom_article}'")
    reponse = scraper_prix(nom_article)
    afficher_resultats(reponse)