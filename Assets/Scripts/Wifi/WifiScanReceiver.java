package com.nhclaessens.vumap;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.net.wifi.WifiManager;
import android.net.wifi.ScanResult;
import java.util.List;

import android.util.Log;
import org.json.JSONArray;
import org.json.JSONObject;
import com.unity3d.player.UnityPlayer;

public class WifiScanReceiver extends BroadcastReceiver {
    @Override
    public void onReceive(Context context, Intent intent) {
        boolean success = intent.getBooleanExtra(WifiManager.EXTRA_RESULTS_UPDATED, false);
        if (success) {
            scanSuccess(context);
            Log.d("WifiScanReceiver", "Scan completed");
        } else {
            // handle scan failure
            Log.d("WifiScanReceiver", "Scan failed");
        }
    }

    private void scanSuccess(Context context) {
        WifiManager wifiManager = (WifiManager) context.getSystemService(Context.WIFI_SERVICE);
        List<ScanResult> results = wifiManager.getScanResults();
        // Process the scan results here
        JSONArray jsonArray = new JSONArray();
        try {
            for (ScanResult result : results) {
                JSONObject jsonObject = new JSONObject();
                jsonObject.put("SSID", result.SSID);
                jsonObject.put("MAC", result.BSSID);
                jsonObject.put("signalStrength", result.level);
                jsonArray.put(jsonObject);
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
        UnityPlayer.UnitySendMessage("WifiManager", "onScanComplete", jsonArray.toString());
    }
}
