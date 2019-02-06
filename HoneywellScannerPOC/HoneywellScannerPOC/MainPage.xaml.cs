using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Honeywell.AIDC.CrossPlatform;
using HoneywellScannerPOC.Services;

namespace HoneywellScannerPOC
{
    //TODO: Implement MVVM Pattern
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}

        void On_ClickedScan(object sender, EventArgs eventArgs)
        {
            //TODO: Once moved to VM, this will be injected via IoC
            var scanService = new ScanService();
            scanService.InitializeService();
            scanService.OnScanComplete += _scanComplete;

            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                scannerReady.IsVisible = true;
            });
        }

        void On_ClickedSave(object sender, EventArgs eventArgs)
        {

        }

        private void _scanComplete(object sender, EventArgs e)
        {
            if(sender is ScanService scanService)
            {
                scanService.OnScanComplete -= _scanComplete;

                var bardCodeDetails = (BarcodeDataArgs)e;

                Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                {
                    scannerReady.IsVisible = false;
                    myLabel.Text = bardCodeDetails.Data;
                });
            }
        }
    }
}
