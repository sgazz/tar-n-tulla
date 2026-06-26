using UnityEngine;

namespace TarTulla.UI
{
    public static class UIViewVisibility
    {
        public static void SetVisible(GameObject panel, bool visible, bool animateShow = false)
        {
            if (panel == null)
                return;

            var transition = panel.GetComponent<UIPanelTransition>();
            if (transition == null)
            {
                panel.SetActive(visible);
                return;
            }

            if (visible)
            {
                if (animateShow)
                    transition.ShowAnimated();
                else
                    transition.ShowInstant();
            }
            else
            {
                transition.HideInstant();
            }
        }
    }
}
