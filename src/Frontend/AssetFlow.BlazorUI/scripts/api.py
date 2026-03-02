from flask import Flask, request, jsonify
from flask_cors import CORS  # Pour autoriser les appels depuis Blazor
import sys
import os

# Ajouter le chemin pour importer price_scraper.py
sys.path.append(os.path.dirname(__file__))
from price_scraper import scraper_prix  # votre fonction principale

app = Flask(__name__)
CORS(app)  # Autorise les requêtes cross-origin (utile en développement)

@app.route('/scrape', methods=['GET'])
def scrape():
    query = request.args.get('q')
    if not query:
        return jsonify({'error': 'Paramètre "q" manquant'}), 400

    try:
        resultat = scraper_prix(query)
        return jsonify(resultat)
    except Exception as e:
        return jsonify({'error': str(e)}), 500

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True)