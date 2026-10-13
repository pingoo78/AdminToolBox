// This file is part of AdminToolBox.
// Copyright (C) 2026 Alexandre ARMENIAN
//
// AdminToolBox is free software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License as published by the Free Software Foundation, either version 3
// of the License, or (at your option) any later version.
//
// AdminToolBox is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with this program.
// If not, see <https://www.gnu.org/licenses/>.

using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Win32;

namespace AdminToolBox.Services;

/// <summary>Photo instantanée des informations réseau/système du poste, utile à un admin en un coup d'œil.</summary>
public sealed record SystemInfoSnapshot(
    string HostName,
    string DomainOrWorkgroup,
    string UserName,
    string OsVersion,
    string IPv4Addresses,
    string DefaultGateway,
    string DnsServers);

public static class SystemInfoService
{
    private const string Inconnu = "—";

    public static SystemInfoSnapshot GetSnapshot()
    {
        var (ips, gateway, dns) = GetPrimaryAdapterInfo();

        return new SystemInfoSnapshot(
            HostName: Dns.GetHostName(),
            DomainOrWorkgroup: GetDomainOrWorkgroup(),
            UserName: Environment.UserName,
            OsVersion: GetOsVersion(),
            IPv4Addresses: ips.Count > 0 ? string.Join("\n", ips) : Inconnu,
            DefaultGateway: gateway ?? Inconnu,
            DnsServers: dns.Count > 0 ? string.Join(", ", dns) : Inconnu);
    }

    private static (List<string> Ips, string? Gateway, List<string> Dns) GetPrimaryAdapterInfo()
    {
        var ips = new List<string>();
        string? gateway = null;
        var dns = new List<string>();

        var activeAdapters = NetworkInterface.GetAllNetworkInterfaces()
            .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
            .Where(nic => nic.NetworkInterfaceType is not (NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel));

        foreach (var nic in activeAdapters)
        {
            var props = nic.GetIPProperties();

            ips.AddRange(props.UnicastAddresses
                .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(a => a.Address.ToString()));

            gateway ??= props.GatewayAddresses
                .FirstOrDefault(g => g.Address.AddressFamily == AddressFamily.InterNetwork)?.Address.ToString();

            dns.AddRange(props.DnsAddresses
                .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                .Select(a => a.ToString()));
        }

        return (ips.Distinct().ToList(), gateway, dns.Distinct().ToList());
    }

    /// <summary>
    /// Win32_ComputerSystem.Domain renvoie le domaine Active Directory si la machine y est jointe,
    /// ou le nom du groupe de travail sinon. Environment.UserDomainName ne convient pas ici : pour
    /// un compte local, il renvoie le nom de l'ordinateur, pas "WORKGROUP".
    /// </summary>
    private static string GetDomainOrWorkgroup()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Domain FROM Win32_ComputerSystem");
            using var results = searcher.Get();
            foreach (ManagementBaseObject result in results)
            {
                using (result)
                {
                    if (result["Domain"] is string domain && !string.IsNullOrWhiteSpace(domain))
                    {
                        return domain;
                    }
                }
            }
        }
        catch (ManagementException)
        {
            // Service WMI indisponible : on retombe sur l'info la moins mauvaise ci-dessous.
        }

        return Environment.UserDomainName;
    }

    /// <summary>
    /// Deux lignes : "Windows 11 Pro 24H2" puis "Build 26100.2033". La clé de registre ProductName
    /// n'a jamais été mise à jour par Microsoft pour Windows 11 (elle contient toujours "Windows
    /// 10 ...", y compris sur une install Windows 11 à jour) : on corrige en se basant sur le
    /// numéro de build (>= 22000 ⇒ Windows 11), seul indicateur fiable.
    /// </summary>
    private static string GetOsVersion()
    {
        using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
        var productName = key?.GetValue("ProductName") as string ?? "Windows";
        var displayVersion = key?.GetValue("DisplayVersion") as string;
        var buildNumber = key?.GetValue("CurrentBuildNumber") as string;
        var ubr = key?.GetValue("UBR");

        if (productName.StartsWith("Windows 10", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(buildNumber, out var build) && build >= 22000)
        {
            productName = "Windows 11" + productName["Windows 10".Length..];
        }

        var firstLine = string.IsNullOrEmpty(displayVersion) ? productName : $"{productName} {displayVersion}";
        var secondLine = buildNumber is null ? null : $"Build {buildNumber}{(ubr is not null ? $".{ubr}" : string.Empty)}";

        return secondLine is null ? firstLine : $"{firstLine}\n{secondLine}";
    }
}
