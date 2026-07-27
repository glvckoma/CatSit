using System.Windows;
using System.Windows.Input;
using CatSit.Core.Learning;

namespace CatSit.UI;

public sealed class TriggerViewModel
{
    public TriggerViewModel(TriggerRecord record, Action<string, TriggerVerdict> report)
    {
        Record = record;
        ConfirmCommand = new RelayCommand(() => report(record.Id, TriggerVerdict.ConfirmedCat));
        RejectCommand = new RelayCommand(() => report(record.Id, TriggerVerdict.FalseAlarm));
    }

    public TriggerRecord Record { get; }

    public ICommand ConfirmCommand { get; }
    public ICommand RejectCommand { get; }

    public string Headline => string.IsNullOrEmpty(Record.Signature)
        ? Record.RuleDescription
        : $"{Record.RuleDescription} ({Record.Signature})";

    public string Detail
    {
        get
        {
            var when = Record.TimestampUtc.ToLocalTime().ToString("MMM d, h:mm tt");
            var action = Record.Locked ? "input locked" : "warned only";
            return $"{when} · {action}";
        }
    }

    public string VerdictText => Record.Verdict switch
    {
        TriggerVerdict.ConfirmedCat => "Confirmed cat",
        TriggerVerdict.FalseAlarm => "Ignored from now on",
        _ => string.Empty
    };

    public Visibility VerdictVisibility =>
        Record.Verdict == TriggerVerdict.Unreviewed ? Visibility.Collapsed : Visibility.Visible;

    public Visibility ButtonsVisibility =>
        Record.Verdict == TriggerVerdict.Unreviewed ? Visibility.Visible : Visibility.Collapsed;
}

public sealed class ExclusionViewModel
{
    public ExclusionViewModel(string signature, Action<string> remove)
    {
        Signature = signature;
        RemoveCommand = new RelayCommand(() => remove(signature));
    }

    public string Signature { get; }
    public ICommand RemoveCommand { get; }
}

public sealed record NamedOption<T>(string Name, T Value);
