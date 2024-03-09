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
            piece.transform.localPosition = Vector3.zero;
            piece.name = piece.name;

            // create a button for the visuals
            GameObject buttonPrefab = Instantiate(cemeteryButtonPrefab);

            // associate it to the dead piece
            buttonPrefab.GetComponent<CemeteryButton>().Setup(piece.GetComponent<Piece>().soPiece.Image, piece);

            // add the button to the cemetery
            buttonPrefab.transform.SetParent(cemetery.transform, false);
        }
    }
}
