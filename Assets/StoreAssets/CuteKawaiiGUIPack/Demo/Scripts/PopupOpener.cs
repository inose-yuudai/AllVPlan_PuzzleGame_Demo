// Copyright (C) 2024 ricimi. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using UnityEngine;

namespace Ricimi
{
    // This class is responsible for creating and opening a popup of the
    // given prefab and adding it to the UI canvas of the current scene.
    public class PopupOpener : MonoBehaviour
    {
        public GameObject popupPrefab;

        protected Canvas m_canvas;
        protected GameObject m_popup;

        protected void Start()
        {
            m_canvas = GetComponentInParent<Canvas>();
        }

        public virtual void OpenPopup()
        {
            m_popup = Instantiate(popupPrefab, m_canvas.transform, false);
            m_popup.SetActive(true);
            m_popup.GetComponent<Popup>().Open();
        }
    }
}
