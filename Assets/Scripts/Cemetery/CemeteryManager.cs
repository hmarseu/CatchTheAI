using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace catchTheAI
{
    public class CemeteryManager : MonoBehaviour
    {
        [SerializeField] private GameObject cemeteryButtonPrefab;
        [SerializeField] private GameObject deadZonePlayer1;
        [SerializeField] private GameObject deadZonePlayer2;
        [SerializeField] private GameObject parentDeadPieces;

        public void AddToCemetery(GameObject piece, int playerId)
        {
            GameObject cemetery = playerId == 1 ? deadZonePlayer1 : deadZonePlayer2;

            piece.transform.SetParent(parentDeadPieces.transform);
            piece.transform.localPosition = Vector3.zero; // Réinitialiser la position locale
            piece.name = piece.name;

            // Créer un bouton pour le visuel
            GameObject buttonPrefab = Instantiate(cemeteryButtonPrefab);

            // Associer le bouton à la piece morte
            buttonPrefab.GetComponent<CemeteryButton>().Setup(piece.GetComponent<Piece>().soPiece.Image, piece);

            // Ajouter le bouton au cimetière
            buttonPrefab.transform.SetParent(cemetery.transform, false);
        }

        // Méthode pour retirer une pièce du cimetière
        public void RemovePieceFromCemetery(GameObject piece, int playerId)
        {
            if (playerId == 1)
                Destroy(piece);
            else if (playerId == 2)
                Destroy(piece);
        }
    }
}
