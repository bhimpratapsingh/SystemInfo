using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

public class Program
{
    public static void Main()
    {
        int taskSelection;
        do
        {
            //Setting console title
            Console.Title = "Process Manager";

            do
            {
                //To clear the console screen
                Console.Clear();

                //Provide options to perform operations
                Console.WriteLine("Please Select an option to perform. \n" +
                                     "1. Start an application or open file \n" +
                                     "2. Check .NET Framework version 1-4 \n" +
                                     "3. Check .NET Framework version 4.5 plus \n" +
                                     "4. Get system information \n" +
                                     "5. Quit");

                if (!int.TryParse(Console.ReadLine(), out taskSelection))
                {
                    Console.WriteLine("Please enter a valid number.\nPress any key to continue...");
                    Console.ReadKey();
                }
            } while (taskSelection == 0);

            switch (taskSelection)
            {
                case 1:
                    StartProcess();
                    break;

                case 2:
                    GetDotNetFrameworkVersion();
                    break;

                case 3:
                    GetDotNetFrameworkVersion45Plus();
                    break;

                case 4:
                    GetSystemInformation();
                    break;

                case 5:
                    Environment.Exit(0);
                    break;
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

        } while (taskSelection != 5);
    }


    #region GetSystemInformation

    private static void GetSystemInformation()
    {
        Console.WriteLine("\n----------System Information----------");
        Console.WriteLine("PC Name: " + Environment.MachineName);
        Console.WriteLine("Machine Name: " + Environment.MachineName);
        Console.WriteLine("Domain Name: " + Environment.UserDomainName);
        Console.WriteLine("User Name: " + Environment.UserName);
        Console.WriteLine("Operating System Name: " + GetOSFriendlyName());
        Console.WriteLine("Operating System Version: " + Environment.OSVersion);
        Console.WriteLine("Operating System Bit type: " + (Environment.Is64BitOperatingSystem == true ? "64 Bit" : "32 Bit"));
        Console.WriteLine("Processor Count: " + (Environment.ProcessorCount));
        Console.WriteLine("Processor Speed: " + GetProcessorSpeed() + "MHz");
        Console.WriteLine("CLR Version: " + Environment.Version);
        Console.WriteLine("MAC Address: " + GetMacAddress());
        Console.WriteLine("Private IP Address: " + GetPrivateIpAddress());

        if (CheckSystemOnline())
        {
            Console.WriteLine("System Online: Yes");
            try
            {
                Console.WriteLine("Public IP Address: " + new WebClient().DownloadString("https://ipinfo.io/ip"));
            }
            catch (Exception)
            {
                Console.WriteLine("Seems issue with public IP address fetching portal");
            }
        }
        else
        {
            Console.WriteLine("System Online: No");
        }

        Console.WriteLine("\nDrives in System:");

        int i = 1;
        foreach (var item in Environment.GetLogicalDrives())
        {
            Console.WriteLine($"{i}. {item}");
            i++;
        }
    }

    // Refer this url https://docs.microsoft.com/en-us/windows/desktop/CIMWin32Prov/win32-processor
    private static string GetProcessorSpeed()
    {
        string clockSpeed = "";

        var searcher = new ManagementObjectSearcher("select MaxClockSpeed from Win32_Processor");

        foreach (var item in searcher.Get())
        {
            clockSpeed = ((uint)item["MaxClockSpeed"]).ToString();
        }

        return clockSpeed;
    }

    private static string GetPrivateIpAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        return "";
    }

    private static bool CheckSystemOnline()
    {
        try
        {
            Dns.GetHostEntry("www.google.com");
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static string GetMacAddress()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
               .Where(x => x.OperationalStatus == OperationalStatus.Up && x.NetworkInterfaceType != NetworkInterfaceType.Loopback)
               .Select(x => x.GetPhysicalAddress().ToString()).FirstOrDefault();
    }

    //Add reference to System.Management
    private static string GetOSFriendlyName()
    {
        string result = string.Empty;
        ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
        foreach (ManagementObject os in searcher.Get().Cast<ManagementObject>())
        {
            result = os["Caption"].ToString();
            break;
        }
        return result;
    }

    #endregion

    #region GetDotNetFrameworkVersion 4.5+

    private static void GetDotNetFrameworkVersion45Plus()
    {
        const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

        using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
        {
            if (ndpKey != null && ndpKey.GetValue("Release") != null)
            {
                Console.WriteLine($".NET Framework Version: {CheckFor45PlusVersion((int)ndpKey.GetValue("Release"))}");
            }
            else
            {
                Console.WriteLine(".NET Framework Version 4.5 or later is not detected.");
            }
        }

        // Checking the version using >= enables forward compatibility.
        string CheckFor45PlusVersion(int releaseKey)
        {
            if (releaseKey >= 528040)
                return "4.8 or later";
            if (releaseKey >= 461808)
                return "4.7.2";
            if (releaseKey >= 461308)
                return "4.7.1";
            if (releaseKey >= 460798)
                return "4.7";
            if (releaseKey >= 394802)
                return "4.6.2";
            if (releaseKey >= 394254)
                return "4.6.1";
            if (releaseKey >= 393295)
                return "4.6";
            if (releaseKey >= 379893)
                return "4.5.2";
            if (releaseKey >= 378675)
                return "4.5.1";
            if (releaseKey >= 378389)
                return "4.5";

            // This code should never execute. A non-null release key should mean
            // that 4.5 or later is installed.
            return "No 4.5 or later version detected";
        }
    }

    #endregion

    #region getDotNetFrameworkVersion 1-4

    private static void GetDotNetFrameworkVersion()
    {
        // Opens the registry key for the .NET Framework entry.
        using (RegistryKey ndpKey =
                RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).
                OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
        {
            foreach (var versionKeyName in ndpKey.GetSubKeyNames())
            {
                // Skip .NET Framework 4.5 version information.
                if (versionKeyName == "v4")
                {
                    continue;
                }

                if (versionKeyName.StartsWith("v"))
                {

                    RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);
                    // Get the .NET Framework version value.
                    var name = (string)versionKey.GetValue("Version", "");
                    // Get the service pack (SP) number.
                    var sp = versionKey.GetValue("SP", "").ToString();

                    // Get the installation flag, or an empty string if there is none.
                    var install = versionKey.GetValue("Install", "").ToString();
                    if (string.IsNullOrEmpty(install)) // No install info; it must be in a child subkey.
                        Console.WriteLine($"{versionKeyName}  {name}");
                    else
                    {
                        if (!(string.IsNullOrEmpty(sp)) && install == "1")
                        {
                            Console.WriteLine($"{versionKeyName}  {name}  SP{sp}");
                        }
                    }
                    if (!string.IsNullOrEmpty(name))
                    {
                        continue;
                    }
                    foreach (var subKeyName in versionKey.GetSubKeyNames())
                    {
                        RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
                        name = (string)subKey.GetValue("Version", "");
                        if (!string.IsNullOrEmpty(name))
                            sp = subKey.GetValue("SP", "").ToString();

                        install = subKey.GetValue("Install", "").ToString();
                        if (string.IsNullOrEmpty(install)) //No install info; it must be later.
                            Console.WriteLine($"{versionKeyName}  {name}");
                        else
                        {
                            if (!(string.IsNullOrEmpty(sp)) && install == "1")
                            {
                                Console.WriteLine($"{subKeyName}  {name}  SP{sp}");
                            }
                            else if (install == "1")
                            {
                                Console.WriteLine($"  {subKeyName}  {name}");
                            }
                        }
                    }
                }
            }
        }
    }

    #endregion

    #region startProcess

    private static void StartProcess()
    {
        Console.WriteLine("Please enter the process name to start or location of the file.");
        string processName = Console.ReadLine();

        try
        {
            Process.Start(processName);
            Console.WriteLine("Process started successfully.");
        }
        catch (Exception)
        {
            Console.WriteLine("Invalid process name entered, or some error occured in starting the process");
        }
    }

    #endregion
}