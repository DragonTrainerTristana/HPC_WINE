using UnityEngine;
using System; // Math 클래스 사용

public class FSO_LinkBudget : MonoBehaviour
{
    [Header("General Link Parameters")]
    [SerializeField] private double propagationDistance_m = 2.0;
    [SerializeField] private double channelBandwidth_GHz = 30.0;

    [Header("FSO Link Parameters")]
    [SerializeField] private double fsoCarrierFrequency_THz = 193.55;
    [SerializeField] private double fsoTransmitPower_dBm = 23.01;
    [SerializeField] private double fsoReceiveLensRadius_mm = 12.0;
    [SerializeField] private double fsoBeamDivergence_mrad = 0.28;
    [SerializeField] private double fsoAtmosphericLoss_dB = 0.0;
    [SerializeField] private double fsoPointingLoss_dB = 5.0;
    [SerializeField] private double fsoNEP_dB = -21.45;
    [SerializeField] private double fsoNoiseFigure_dB = 15.0;
    [SerializeField] private double fsoModulationLoss_dB = 3.0;

    [Header("Results (FSO)")]
    [SerializeField] private double fsoReceivedSignalPower_dBm;
    [SerializeField] private double fsoGeometricLoss_dB;
    [SerializeField] private double fsoTotalNoisePower_dBm;
    [SerializeField] private double fsoSNR_dB;
    [SerializeField] private double fsoSNR_PostModLoss_dB;
    [SerializeField] private double fsoAchievableCapacity_Gbps;

    private double CalculateShannonCapacity_Gbps(double bandwidth_Hz, double snr_linear)
    {
        if (snr_linear <= 0)
        {
            Debug.LogWarning("SNR must be greater than 0 for log2 calculation.");
            return 0;
        }
        return (bandwidth_Hz * Math.Log(1 + snr_linear, 2)) / 1e9;
    }

    [ContextMenu("Calculate FSO Link Budget")]
    public void CalculateFSOLinkBudget()
    {
        double receiveLensRadius_m = fsoReceiveLensRadius_mm * 1e-3;
        double beamDivergence_rad = fsoBeamDivergence_mrad * 1e-3;
        double bandwidth_Hz = channelBandwidth_GHz * 1e9;

        double beamRadiusAtDistance = (beamDivergence_rad * propagationDistance_m) / 2.0;
        if (beamRadiusAtDistance <= 0) beamRadiusAtDistance = 1e-9;

        double areaRatio = (Math.PI * Math.Pow(receiveLensRadius_m, 2)) / (Math.PI * Math.Pow(beamRadiusAtDistance, 2));
        fsoGeometricLoss_dB = -10.0 * Math.Log10(areaRatio);

        fsoReceivedSignalPower_dBm = fsoTransmitPower_dBm - fsoGeometricLoss_dB - fsoAtmosphericLoss_dB - fsoPointingLoss_dB;

        fsoTotalNoisePower_dBm = fsoNEP_dB + fsoNoiseFigure_dB;

        fsoSNR_dB = fsoReceivedSignalPower_dBm - fsoTotalNoisePower_dBm;
        fsoSNR_PostModLoss_dB = fsoSNR_dB - fsoModulationLoss_dB;

        double snr_linear = Math.Pow(10.0, fsoSNR_PostModLoss_dB / 10.0);
        fsoAchievableCapacity_Gbps = CalculateShannonCapacity_Gbps(bandwidth_Hz, snr_linear);

        Debug.Log("--- FSO Link Budget ---");
        Debug.Log($"Received Power:   {fsoReceivedSignalPower_dBm:F2} dBm");
        Debug.Log($"Geometric Loss:   {fsoGeometricLoss_dB:F2} dB");
        Debug.Log($"Total Noise:      {fsoTotalNoisePower_dBm:F2} dBm");
        Debug.Log($"Raw SNR:          {fsoSNR_dB:F2} dB");
        Debug.Log($"Mod Loss SNR:     {fsoSNR_PostModLoss_dB:F2} dB");
        Debug.Log($"Capacity:         {fsoAchievableCapacity_Gbps:F2} Gbps");
    }
}