import pandas as pd
import os
import shutil

def generateVidyaviharSchedule():
    # 1. Define Paths for both PC development and Mobile bundling
    # PC Path: Used for Unity Editor testing
    pc_data_dir = r"C:\Users\kenneth ornello\My project\Assets\data"
    # Mobile Path: Bundled into the APK for your phone
    mobile_data_dir = r"C:\Users\kenneth ornello\My project\Assets\StreamingAssets"

    # Ensure both directories exist
    for directory in [pc_data_dir, mobile_data_dir]:
        if not os.path.exists(directory):
            os.makedirs(directory)
    
    pc_output_path = os.path.join(pc_data_dir, "Vidyavihar_Arrivals_2026.csv")
    mobile_output_path = os.path.join(mobile_data_dir, "Vidyavihar_Arrivals_2026.csv")

    # 2. Hard-coded Verified 2026 Slow Train Data (Central Line)
    # PF 1 = Down (Kalyan Side), PF 2 = Up (CSMT Side)
    data = [
        # --- PLATFORM 1 (DOWN LINE) ---
        ["96601 CTL", "04:45 AM", "PF1", "Titwala (Starts)"],
        ["97303 CT 1", "04:58 AM", "PF1", "Thane"],
        ["96303 A 3", "05:16 AM", "PF1", "Ambarnath"],
        ["95701 K 3", "05:20 AM", "PF1", "Kalyan"],
        ["96201 CBL", "05:26 AM", "PF1", "Badlapur (Starts)"],
        ["97005 CK 1", "05:34 AM", "PF1", "Kalyan (Starts)"],
        ["96607 TL 5", "06:14 AM", "PF1", "Titwala"],
        ["96205 CK 9", "06:37 AM", "PF1", "Kalyan (Starts)"],
        ["96311 CA 1", "06:49 AM", "PF1", "Ambarnath (Starts)"],
        ["97011 CK 11", "07:23 AM", "PF1", "Kalyan (Starts)"],
        ["97019 K 19", "07:53 AM", "PF1", "Kalyan"],
        ["97313 T 13", "07:58 AM", "PF1", "Thane"],
        ["97333 T 33", "09:30 AM", "PF1", "Thane"],
        ["97059 K 59", "12:52 PM", "PF1", "Kalyan"],
        ["96419 N 19", "01:10 PM", "PF1", "Kasara"],
        ["96227 BL 27", "01:50 PM", "PF1", "Badlapur"],
        ["97071 K 71", "02:20 PM", "PF1", "Kalyan"],
        ["97401 T 101", "06:26 PM", "PF1", "Thane"],
        ["96635 TL 5", "06:57 PM", "PF1", "Titwala"],
        ["96435 N 35", "10:47 PM", "PF1", "Kasara"],
        ["96261 BL 61", "11:51 PM", "PF1", "Badlapur"],

        # --- PLATFORM 2 (UP LINE) ---
        ["96102 S 2", "02:30 AM", "PF2", "CSMT"],
        ["97002 K 2", "05:12 AM", "PF2", "CSMT"],
        ["97302 T 2", "05:38 AM", "PF2", "CSMT"],
        ["96302 A 2", "06:04 AM", "PF2", "CSMT"],
        ["96602 TL 2", "06:22 AM", "PF2", "CSMT"],
        ["97004 PK 4", "06:54 AM", "PF2", "Parel"],
        ["97020 K 20", "07:21 AM", "PF2", "CSMT"],
        ["96602 PTL 2", "08:09 AM", "PF2", "Parel"],
        ["96124 S 24", "10:43 AM", "PF2", "CSMT"],
        ["97016 PK 16", "11:42 AM", "PF2", "Parel"],
        ["96212 DBL 2", "12:27 PM", "PF2", "Dadar"],
        ["96240 BL 40", "01:56 PM", "PF2", "CSMT"],
        ["97014 TK 14", "03:58 PM", "PF2", "Thane Terminate"],
        ["97056 K 56", "04:04 PM", "PF2", "CSMT"],
        ["97402 T 102", "05:13 PM", "PF2", "CSMT"],
        ["97408 T 108", "05:31 PM", "PF2", "CSMT"],
        ["96310 DA 10", "07:12 PM", "PF2", "Dadar"],
        ["97130 DK 12", "08:41 PM", "PF2", "Dadar"],
        ["97100 CK 10", "10:56 PM", "PF2", "Kurla"]
    ]

    # 3. Create DataFrame and Save with UTF-8-SIG (Handles hidden BOM for Phone reading)
    df = pd.DataFrame(data, columns=["Train_No", "Time", "Platform", "Destination"])
    
    # Save to both locations
    df.to_csv(pc_output_path, index=False, encoding='utf-8-sig')
    df.to_csv(mobile_output_path, index=False, encoding='utf-8-sig')
    
    print(f"--- SUCCESS ---")
    print(f"Data Synced to PC: {pc_output_path}")
    print(f"Data Synced to Mobile: {mobile_output_path}")
    print(f"Total Trains: {len(df)}")

if __name__ == "__main__":
    generateVidyaviharSchedule()