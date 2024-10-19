package com.nhclaessens.vumap;

import com.nhclaessens.vumap.WifiScanReceiver;

import android.content.Context;
import android.content.IntentFilter;
import android.content.Intent;
import android.util.Log;
import android.net.wifi.WifiManager;

public class WifiManagerPlugin {
    private WifiScanReceiver wifiScanReceiver = new WifiScanReceiver();
    private WifiManager wifiManager;

    public void test() {
        Log.d("WifiManagerPlugin", "###################### hello");
    }

    public void registerWifiScanReceiver(Context context) {
        IntentFilter intentFilter = new IntentFilter();
        intentFilter.addAction(WifiManager.SCAN_RESULTS_AVAILABLE_ACTION);
        Intent test = context.registerReceiver(wifiScanReceiver, intentFilter);
        Log.d("WifiManagerPlugin", "Registered receiver ");
    }

    public void unregisterWifiScanReceiver(Context context) {
        context.unregisterReceiver(wifiScanReceiver);
    }

    public void startScan(Context context) {
        if(wifiManager == null) {
            wifiManager = (WifiManager) context.getSystemService(Context.WIFI_SERVICE);
        }
        wifiManager.startScan();
        Log.d("WifiManagerPlugin", "Started scan");
    }
}

