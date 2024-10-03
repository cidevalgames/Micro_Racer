# Micro Racers

## Structure des dossiers
- <ins>Art :</ins> un dossier pour stocker les assets artistiques originaux
- <ins>Audio :</ins> un dossier pour stocker les assets audio originaux
- <ins>Design Documents :</ins> un dossier contenant un projet Unity 2022.3.46f1 configuré pour de l'URP en 3D

## Importer le projet Unity
- Lancez Unity HUB et sélectionnez l'option **Add project from disk** et naviguez vers le dossier *Unity Project/Vroum Unity Project*.

Sentez-vous libre d'ajouter de nouveaux dossiers si nécessaire.

S'il-vous-plaît notez qu'une structure de dossiers similaire existe dans le dossier du projet Unity qui devrait contenir les versions optimisées des assets artistiques et audio pour leur usage dans le jeu.

## Règles du repository
### Workflow
- Les commits sont écrits en français (pas d'anglais)
- Les commits doivent être explicites (pas de "Update", ni de "Commit", ni de "Session du 10/01/2025"). Mettez ce que vous avez fait pendant la journée, pas ce que vous êtes en train de faire.
- **<ins>Pour GD :</ins>** Créer une branche par feature après avoir pull depuis la branche **develop** (nomenclature : feature-la_feature_en_question). Pas de branche à son nom.
- **<ins>Pour GA :</ins>** Créer une branche par action après avoir pull depuis la branche **develop** (ex : art-intégration_modèle_voiture)
- Travailler seulement dans la branche de la feature tant que la feature n'est pas finie
- Créer une pull request pour push une version finale de la feature dans la branche **develop** (Vallérian vérifie le code avant d'accepter la pull request)

### Général
- Demander à Vallérian de l'aide pour être sûr de ne pas faire de bêtises
- Publier un commit quotidiennement
- Les commits ne respectant pas les règles seront soit refaits par Vallérian, soit refusés (si non respect total des règles)
- Il est **<ins>INTERDIT</ins>** de push dans la branche **main**
