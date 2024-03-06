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

        public void clickOnCemeteryPiece()
        {
           selectPiece(associatedPiece);
        }

        public void Setup(Sprite image, GameObject piece)
        {
            associatedPiece = piece;
            pieceImage = image;
            GetComponent<Button>().onClick.AddListener(clickOnCemeteryPiece);
            this.gameObject.GetComponent<Image>().sprite = pieceImage;
        }
    }
}
