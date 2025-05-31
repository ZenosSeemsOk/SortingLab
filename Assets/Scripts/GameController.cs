using UnityEngine;

public class GameController : MonoBehaviour
{
    public ColorChanger FirstBottle;
    public ColorChanger SecondBottle;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider == null) return;

            ColorChanger clickedBottle = hit.collider.GetComponent<ColorChanger>();
            if (clickedBottle == null) return;

            if (FirstBottle == null)
            {
                FirstBottle = clickedBottle;
            }
            else if (FirstBottle == clickedBottle)
            {
                FirstBottle = null;
            }
            else
            {
                SecondBottle = clickedBottle;
                FirstBottle.colorChangerRef = SecondBottle;

                FirstBottle.UpdateTopColorValues();
                SecondBottle.UpdateTopColorValues();

                if (SecondBottle.FillBottleCheck(FirstBottle.topColor))
                {
                    FirstBottle.StartColorTransfer();
                }

                FirstBottle = null;
                SecondBottle = null;
            }
        }
    }
}
