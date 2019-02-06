using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Honeywell.AIDC.CrossPlatform;

namespace HoneywellScannerPOC.Services
{
    //TODO: This will be abstracted to an interface
    public class ScanService 
    {
        private BarcodeReader _barcodeReader;
        /// <summary>
        /// WARNING: You must call InitializeService() before subscrbing to this EventHandler
        /// </summary>
        public EventHandler OnScanComplete { get; set; }

        /// <summary>
        /// InitializeService must be called before subscribing to OnScanComplete
        /// </summary>
        public void InitializeService()
        {
            Task.Run(async () =>
            {
                await GetReaderAsync();
                await InitializeReaderAsync();
            });
        }

        private async Task InitializeReaderAsync()
        {
            if (_barcodeReader != null)
            {
                await OpenReaderAsync();
                await SetReaderSettingsAsync();
            }
        }

        private async Task SetReaderSettingsAsync()
        {
            if (_barcodeReader.IsReaderOpened)
            {
                Dictionary<string, object> settings = new Dictionary<string, object>()
                    {
                        {_barcodeReader.SettingKeys.TriggerScanMode, _barcodeReader.SettingValues.TriggerScanMode_OneShot },
                        {_barcodeReader.SettingKeys.Code128Enabled, true },
                        {_barcodeReader.SettingKeys.Code39Enabled, true },
                        {_barcodeReader.SettingKeys.Ean8Enabled, true },
                        {_barcodeReader.SettingKeys.Ean8CheckDigitTransmitEnabled, true },
                        {_barcodeReader.SettingKeys.Ean13Enabled, true },
                        {_barcodeReader.SettingKeys.Ean13CheckDigitTransmitEnabled, true },
                        {_barcodeReader.SettingKeys.Interleaved25Enabled, true },
                        {_barcodeReader.SettingKeys.Interleaved25MaximumLength, 100 },
                        {_barcodeReader.SettingKeys.Postal2DMode, _barcodeReader.SettingValues.Postal2DMode_Usps }
                    };

                BarcodeReader.Result result = await _barcodeReader.SetAsync(settings);
                if (result.Code != BarcodeReader.Result.Codes.SUCCESS)
                {
                    throw new InvalidOperationException("Failure to set settings for barcode scanner");
                }
            }
        }

        private async Task OpenReaderAsync()
        {
            BarcodeReader.Result openResult = await _barcodeReader.OpenAsync();
        }

        private async Task GetReaderAsync()
        {
            IList<BarcodeReaderInfo> readerInfoList = await BarcodeReader.GetConnectedBarcodeReaders();
            var readerName = readerInfoList.FirstOrDefault().ScannerName;
            _barcodeReader = new BarcodeReader(readerName);

            _barcodeReader.BarcodeDataReady += _barcodeReader_BarcodeDataReady;
        }

        /// <summary>
        /// WPF and Xamarin allow for event handlers to use async void
        /// It's possible that in the long run, it might just be better to wrap the await CloseReaderAsync();
        /// in a Task.Run(async()=>await CloseReaderAsync()); and remove the async?
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void _barcodeReader_BarcodeDataReady(object sender, BarcodeDataArgs e)
        {
            _barcodeReader.BarcodeDataReady -= _barcodeReader_BarcodeDataReady;

            await CloseReaderAsync();

            OnScanComplete.Invoke(this, e);
        }

        private async Task CloseReaderAsync()
        {
            await _barcodeReader.CloseAsync();
        }
    }
}
