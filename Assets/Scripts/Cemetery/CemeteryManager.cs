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
            piece.transform.localPosition = Vector3.zero; // R�initialiser la position locale
            piece.name = piece.name;

            // Cr�er un bouton pour le visuel
            GameObject buttonPrefab = Instantiate(cemeteryButtonPrefab);

            // Associer le bouton � la piece morte
            buttonPrefab.GetComponent<CemeteryButton>().Setup(piece.GetComponent<Piece>().soPiece.Image, piece);

            // Ajouter le bouton au cimeti�re
            buttonPrefab.transform.SetParent(cemetery.transform, false);
        }
    }
}
