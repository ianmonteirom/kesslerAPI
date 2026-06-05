namespace Kessler.Domain.Constants;

/// <summary>
/// Regras e limites de negócio para missões espaciais.
/// </summary>
public static class MissionConstants
{
    /// <summary>Duração mínima de uma missão em dias.</summary>
    public const int MinDurationDays = 1;

    /// <summary>Duração máxima de uma missão em dias (aprox. 5 anos).</summary>
    public const int MaxDurationDays = 1_825;

    /// <summary>Intervalo padrão entre checkpoints de progresso (dias).</summary>
    public const int DefaultCheckpointIntervalDays = 7;

    /// <summary>Número padrão de janelas de alerta geradas por análise.</summary>
    public const int DefaultAlertWindowsCount = 6;

    /// <summary>Intervalo padrão entre janelas de alerta (horas).</summary>
    public const int DefaultAlertIntervalHours = 4;

    /// <summary>Hora UTC de início da janela de alta atenção (menor cobertura de rastreamento).</summary>
    public const int HighAttentionWindowStartHour = 0;

    /// <summary>Hora UTC de fim da janela de alta atenção.</summary>
    public const int HighAttentionWindowEndHour = 6;
}
