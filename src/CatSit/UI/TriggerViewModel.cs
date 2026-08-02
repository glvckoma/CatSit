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
            var action = Record.SuppressedByLearning
                ? "prevented by learning"
                : Record.Locked ? "input locked" : "warned only";
            return $"{when} · {action}";
        }
    }

    public string ConfirmButtonText => Record.Verdict == TriggerVerdict.ConfirmedCat
        ? "Cat (selected)"
        : "Was a cat";

    public string RejectButtonText => Record.Verdict == TriggerVerdict.FalseAlarm
        ? "Not a cat (selected)"
        : "Not a cat";
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

public sealed record NamedOption<T>(string Name, T Value)
{
    public override string ToString() => Name;
}
