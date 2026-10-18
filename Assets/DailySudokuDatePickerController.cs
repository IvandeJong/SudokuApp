using System;
using System.Collections;
using System.Linq;
using UI.Dates;
using UnityEngine;

public class DailySudokuDatePickerController : MonoBehaviour
{
    private const string PopupResourcePath = "Prefabs/DatePicker - Popup";

    private SudokuGameManager gameManager;
    private DatePicker datePicker;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AttachToDailyPicker()
    {
        SudokuGameManager manager = FindFirstObjectByType<SudokuGameManager>();
        if (manager == null)
        {
            return;
        }

        DatePicker picker = FindObjectsByType<DatePicker>(
                FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(candidate => candidate.gameObject.scene == manager.gameObject.scene);

        if (picker != null && picker.GetComponent<DailySudokuDatePickerController>() == null)
        {
            picker.gameObject.AddComponent<DailySudokuDatePickerController>();
        }
    }

    private IEnumerator Start()
    {
        // SudokuGameManager determines the active game mode in Start.
        yield return null;

        gameManager = FindFirstObjectByType<SudokuGameManager>();
        if (gameManager == null || !gameManager.IsDailyGame)
        {
            Destroy(this);
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
        config.DateRange.RestrictToDate = true;
        config.DateRange.ToDate = new SerializableDate(DateTime.Today);
        DatePickerEventConfig eventConfig = config.Events;
        if (eventConfig.OnDaySelected == null)
        {
            eventConfig.OnDaySelected = new DatePickerEvent();
        }
        config.Events = eventConfig;
        datePicker.Config = config;
        datePicker.StatusBadgeColorForDate = gameManager.GetDailyDateBadgeColor;

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
