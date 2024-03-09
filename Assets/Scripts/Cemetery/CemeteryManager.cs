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

        public void SetCemeteryButtonsInteractability(bool player1IsPlaying)
        {
            CemeteryButton[] cemeteryButtonsPlayer1 = deadZonePlayer1.GetComponentsInChildren<CemeteryButton>();
            CemeteryButton[] cemeteryButtonsPlayer2 = deadZonePlayer2.GetComponentsInChildren<CemeteryButton>();

            foreach (CemeteryButton button in cemeteryButtonsPlayer1)
            {
                button.SetInteractability(player1IsPlaying);
                Debug.Log("cemetery 1 : " + player1IsPlaying);
            }
            foreach (CemeteryButton button in cemeteryButtonsPlayer2)
            {
                button.SetInteractability(!player1IsPlaying);
                Debug.Log("cemetery 2 : " + !player1IsPlaying);
            }
        }

    }
}