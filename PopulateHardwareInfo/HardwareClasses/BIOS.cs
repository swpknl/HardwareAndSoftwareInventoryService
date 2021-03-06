﻿namespace PopulateWMIInfo.HardwareClasses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management;

    using Constants;

    using Entities;

    using Helpers;

    using Newtonsoft.Json.Linq;

    using PopulateWMIInfo.Contracts;

    using ReportToRestEndpoint.Contracts;

    /// <summary>
    /// The WMI info for BIOS.
    /// </summary>
    public class BIOS : IWmiInfo
    {
        private const string BiosTableName = "bios";

        private const string BiosClientTableName = "x_client_bios";

        private readonly ManagementObjectSearcher searcher;

        private int id;

        private List<BIOSInfo> biosInfoList;

        /// <summary>
        /// Initializes a new instance of the <see cref="BIOS"/> class.
        /// </summary>
        public BIOS()
        {
            this.biosInfoList = new List<BIOSInfo>();
            this.searcher = new ManagementObjectSearcher(
                WmiConstants.WmiRootNamespace,
                    "SELECT * FROM Win32_BIOS");
        }

        /// <summary>
        /// The get WMI info.
        /// </summary>
        public void GetWMIInfo()
        {
            this.biosInfoList = this.GetValue();
        }
        
        /// <summary>
        /// The report WMI info.
        /// </summary>
        /// <param name="visitor">
        /// The visitor.
        /// </param>
        public void ReportWMIInfo(IVisitor visitor)
        {
            var data = this.ConvertBiosInfoToJson(this.biosInfoList);
            visitor.Visit(BiosTableName, data, out id);
            data = this.ConvertClientBiosInfoToJson(this.biosInfoList);
            int temp;
            visitor.Visit(BiosClientTableName, data, out temp);
        }

        /// <summary>
        /// The check for hardware changes.
        /// </summary>
        /// <param name="visitor">
        /// The visitor.
        /// </param>
        public void CheckForHardwareChanges(IVisitor visitor)
        {
            var changedBiosInfoList = new List<BIOSInfo>();
            var tempBiosInfoList = this.GetValue();

            changedBiosInfoList = tempBiosInfoList.GetDifference(this.biosInfoList);
            if (changedBiosInfoList.Any())
            {
                var data = this.ConvertBiosInfoToJson(changedBiosInfoList);
                visitor.Visit(BiosTableName, data, out id);
                data = this.ConvertClientBiosInfoToJson(changedBiosInfoList);
                int temp;
                visitor.Visit(BiosClientTableName, data, out temp);
            }
        }

        /// <summary>
        /// The get value.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        private List<BIOSInfo> GetValue()
        {
            var tempList = new List<BIOSInfo>();

            // TODO: SystemBIOSMajorVersion and SystemBiosMinorVersion are not present before Windows 10
            try
            {
                DateTime temp;
                foreach (var queryObj in this.searcher.Get())
                {
                    var biosInfo = new BIOSInfo
                    {
                        Manufacturer = queryObj[WmiConstants.Manufacturer],
                        Version = queryObj[WmiConstants.Version],
                        SMBIOSBIOSVersion = queryObj[WmiConstants.SMBIOSBIOSVersion],
                        SMBIOSMajorVersion = queryObj[WmiConstants.SMBIOSMajorVersion],
                        SMBIOSMinorVersion = queryObj[WmiConstants.SMBIOSMinorVersion],
                        SerialNumber = queryObj[WmiConstants.SerialNumber]
                    };
                    if (DateTime.TryParse(queryObj[WmiConstants.ReleaseDate].ToString(), out temp))
                    {
                        biosInfo.ReleaseDate = temp.ToLocalTime();
                    }
                    else
                    {
                        biosInfo.ReleaseDate = null;
                    }

                    tempList.Add(biosInfo);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return tempList;
        }

        /// <summary>
        /// The convert bios info to json.
        /// </summary>
        /// <param name="biosInfoList">
        /// The bios info list.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string ConvertBiosInfoToJson(List<BIOSInfo> biosInfoList)
        {
            var data = new JObject(
                new JProperty(
                    "resource",
                    new JArray(
                        from biosInfo in biosInfoList
                        select
                            new JObject(
                            new JProperty("manufacturer", biosInfo.Manufacturer),
                            new JProperty("major_version", biosInfo.SystemBIOSMajorVersion),
                            new JProperty("minor_version", biosInfo.SystemBIOSMinorVersion),
                            new JProperty("sm_version", biosInfo.SMBIOSBIOSVersion),
                            new JProperty("sm_major_version", biosInfo.SMBIOSMajorVersion),
                            new JProperty("sm_minor_versions", biosInfo.SMBIOSMinorVersion),
                            new JProperty("release_date", biosInfo.ReleaseDate)))));
            return data.ToString();
        }

        /// <summary>
        /// The convert client bios info to JSON.
        /// </summary>
        /// <param name="biosInfoList">
        /// The bios info list.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string ConvertClientBiosInfoToJson(List<BIOSInfo> biosInfoList)
        {
            var data =
                new JObject(
                    new JProperty(
                        "resource",
                        new JArray(
                            from biosInfo in biosInfoList
                            select new JObject(
                                new JProperty("serial_number", biosInfo.SerialNumber),
                                new JProperty("client_id", ClientId.Id),
                                new JProperty("bios_id", this.id)))));
            return data.ToString();
        }
    }
}
