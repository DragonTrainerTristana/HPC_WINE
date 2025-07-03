using UnityEngine;
using System; // Math 클래스 사용

public class THZ_LinkBudget : MonoBehaviour
{
    [Header("Constants")]
    private const double SpeedOfLight = 299792458.0; // m/s
    private const double BoltzmannConstant = 1.380649e-23; // J/K

    [Header("General Link Parameters")]
    [SerializeField] private double propagationDistance_m = 2.0;
    [SerializeField] private double channelBandwidth_GHz = 30.0;
    [SerializeField] private double ambientTemperature_K = 293.0;

    [Header("THz Link Parameters")]
    [SerializeField] private double thzCarrierFrequency_THz = 0.30;
    [SerializeField] private double thzTransmitPower_dBm = 13.0;
    [SerializeField] private double thzTransmitGain_dBi = 32.1;
    [SerializeField] private double thzReceiveGain_dBi = 32.1;
    [SerializeField] private double thzAtmosphericLoss_dB = 0.008;
    [SerializeField] private double thzNoiseFigure_dB = 12.0;
    [SerializeField] private double thzModulationLoss_dB = 3.6;

    [Header("Results (THz)")]
    [SerializeField] private double thzReceivedSignalPower_dBm;
    [SerializeField] private double thzThermalNoise_dBm;
    [SerializeField] private double thzTotalNoisePower_dBm;
    [SerializeField] private double thzSNR_dB;
    [SerializeField] private double thzSNR_PostModLoss_dB;
    [SerializeField] private double thzAchievableCapacity_Gbps;

    private double CalculateShannonCapacity_Gbps(double bandwidth_Hz, double snr_linear)
    {
        if (snr_linear <= 0)
        {
            Debug.LogWarning("SNR must be greater than 0 for log2 calculation.");
            return 0;
        }
        return (bandwidth_Hz * Math.Log(1 + snr_linear, 2)) / 1e9;
    }

    [ContextMenu("Calculate THz Link Budget")]
    public void CalculateTHzLinkBudget()
    {
        double frequency_Hz = thzCarrierFrequency_THz * 1e12;
        double bandwidth_Hz = channelBandwidth_GHz * 1e9;

        double pathLoss_dB = 20.0 * Math.Log10((4.0 * Math.PI * frequency_Hz * propagationDistance_m) / SpeedOfLight);

        thzReceivedSignalPower_dBm = thzTransmitPower_dBm + thzTransmitGain_dBi + thzReceiveGain_dBi - pathLoss_dB - thzAtmosphericLoss_dB;

        double thermalNoise_Watts = BoltzmannConstant * ambientTemperature_K * bandwidth_Hz;
        thzThermalNoise_dBm = 10.0 * Math.Log10(thermalNoise_Watts / 0.001);

        thzTotalNoisePower_dBm = thzThermalNoise_dBm + thzNoiseFigure_dB;

        thzSNR_dB = thzReceivedSignalPower_dBm - thzTotalNoisePower_dBm;
        thzSNR_PostModLoss_dB = thzSNR_dB - thzModulationLoss_dB;

        double snr_linear = Math.Pow(10.0, thzSNR_PostModLoss_dB / 10.0);
        thzAchievableCapacity_Gbps = CalculateShannonCapacity_Gbps(bandwidth_Hz, snr_linear);

        Debug.Log("--- THz Link Budget ---");
        Debug.Log($"Received Power: {thzReceivedSignalPower_dBm:F2} dBm");
        Debug.Log($"Thermal Noise:  {thzThermalNoise_dBm:F2} dBm");
        Debug.Log($"Total Noise:    {thzTotalNoisePower_dBm:F2} dBm");
        Debug.Log($"Raw SNR:        {thzSNR_dB:F2} dB");
        Debug.Log($"Mod Loss SNR:   {thzSNR_PostModLoss_dB:F2} dB");
        Debug.Log($"Capacity:       {thzAchievableCapacity_Gbps:F2} Gbps");
    }
}