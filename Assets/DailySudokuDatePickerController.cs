using System;
using System.Collections;
using UI.Dates;
using UnityEngine;

public class DailySudokuDatePickerController : MonoBehaviour
{
    private const string PopupResourcePath = "Prefabs/DatePicker - Popup";

    private SudokuGameManager gameManager;
    private DatePicker datePicker;

    private IEnumerator Start()
    {
        // SudokuGameManager determines the active game mode in Start.
        yield return null;

        gameManager = FindFirstObjectByType<SudokuGameManager>();
        if (gameManager == null || !gameManager.IsDailyGame)
        {
            gameObject.SetActive(false);
            yield break;
        }

        datePicker = GetComponentInChildren<DatePicker>(true);
        if (datePicker == null)
        {
            GameObject popupPrefab = Resources.Load<GameObject>(PopupResourcePath);
            if (popupPrefab == null)
            {
                Debug.LogError($"Daily date picker prefab was not found at Resources/{PopupResourcePath}.", this);
                yield break;
            }

            Transform host = transform.Find("Panel");
            GameObject popup = Instantiate(popupPrefab, host != null ? host : transform, false);
            datePicker = popup.GetComponent<DatePicker>();
        }

        if (datePicker == null)
        {
            Debug.LogError("DailyDatePicker does not contain a UI.Dates.DatePicker component.", this);
            yield break;
        }

        DatePickerConfig config = datePicker.Config;
        DatePickerEventConfig eventConfig = config.Events;
        if (eventConfig.OnDaySelected == null)
        {
            eventConfig.OnDaySelected = new DatePickerEvent();
        }
        config.Events = eventConfig;
        datePicker.Config = config;

        datePicker.Config.Events.OnDaySelected.RemoveListener(OnDateSelected);
        datePicker.Config.Events.OnDaySelected.AddListener(OnDateSelected);

        if (DateTime.TryParseExact(
            gameManager.CurrentDailyDate,
            "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out DateTime selectedDate))
        {
            datePicker.SelectedDate = new SerializableDate(selectedDate);
            datePicker.VisibleDate = new SerializableDate(selectedDate);
        }
    }

    private void OnDateSelected(DateTime selectedDate)
    {
        gameManager?.SelectDailyDate(selectedDate.Date);
    }

    private void OnDestroy()
    {
        if (datePicker != null && datePicker.Config.Events.OnDaySelected != null)
        {
            datePicker.Config.Events.OnDaySelected.RemoveListener(OnDateSelected);
        }
    }
}
