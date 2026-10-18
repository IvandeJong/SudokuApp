using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UI.Tables;
using TMPro;

namespace UI.Dates
{
    public class DatePicker_DayButton : DatePicker_Button
    {
        private TextMeshProUGUI statusBadge;

        public DateTime Date;
        public DatePickerDayButtonType Type;

        [HideInInspector]
        public DatePicker DatePicker;

        public void Clicked()
        {
            if (!Button.interactable) return;

            DatePicker.DayButtonClicked(Date);
        }

        public void MouseOver()
        {
            if (!Button.interactable) return;

            DatePicker.DayButtonMouseOver(Date);
        }

        public void SetStatusBadge(Color? color)
        {
            if (statusBadge == null && color.HasValue)
            {
                GameObject badgeObject = new GameObject("Daily Status Badge", typeof(RectTransform));
                RectTransform badgeTransform = badgeObject.GetComponent<RectTransform>();
                badgeTransform.SetParent(Button.transform, false);
                badgeTransform.anchorMin = new Vector2(1f, 1f);
                badgeTransform.anchorMax = new Vector2(1f, 1f);
                badgeTransform.pivot = new Vector2(1f, 1f);
                badgeTransform.anchoredPosition = new Vector2(-1f, -1f);
                badgeTransform.sizeDelta = new Vector2(14f, 14f);

                statusBadge = badgeObject.AddComponent<TextMeshProUGUI>();
                statusBadge.font = Resources.Load<TMP_FontAsset>("Arial SDF");
                statusBadge.text = "•";
                statusBadge.fontSize = 32f;
                statusBadge.alignment = TextAlignmentOptions.Center;
                statusBadge.raycastTarget = false;
            }

            if (statusBadge != null)
            {
                statusBadge.gameObject.SetActive(color.HasValue);
                if (color.HasValue)
                {
                    statusBadge.color = color.Value;
                    statusBadge.transform.SetAsLastSibling();
                }
            }
        }
    }
}
