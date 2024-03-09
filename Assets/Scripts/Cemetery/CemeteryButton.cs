using System.ComponentModel;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace catchTheAI
{
    public class CemeteryButton : MonoBehaviour
    {
        public Sprite pieceImage;
        public GameObject associatedPiece;

        public delegate void CemeteryClicked(GameObject associatedPiece);
        public static event CemeteryClicked selectPiece;

        public void ClickOnCemeteryPiece()
        {
            selectPiece(associatedPiece);
        }

        private void OnEnable()
        {
            BoardManager.removeButtonCemetary += RemoveButton;
        }

        private void OnDisable()
        {
            BoardManager.removeButtonCemetary -= RemoveButton;
        }

        public void Setup(Sprite image, GameObject piece)
        {
            associatedPiece = piece;
            pieceImage = image;
            GetComponent<Button>().onClick.AddListener(ClickOnCemeteryPiece);
            this.gameObject.GetComponent<Image>().sprite = pieceImage;
        }

        private void RemoveButton(GameObject selectedPiece)
        {
            if (selectedPiece == associatedPiece)
            {
                Destroy(this.gameObject);
            }
        }

        public void SetInteractability(bool isInteractable)
        {
            GetComponent<Button>().interactable = isInteractable;
        }
    }
}