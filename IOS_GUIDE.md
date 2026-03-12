# iOS Platform Support Guide

Complete guide for building and deploying **Robot Tower Defense** on iOS devices (iPhone and iPad).

---

## 📱 Overview

This guide covers iOS-specific features, build configuration, and platform optimizations for Robot Tower Defense. The iOS version includes support for:

- **iPhone and iPad** (Universal app)
- **iOS 13.0+** (minimum supported version)
- **Notch/Safe Area** support for modern iPhones
- **Touch gestures** optimized for iOS
- **Haptic feedback** integration
- **Game Center** integration
- **In-App Purchases** via App Store
- **Unity Ads** monetization

---

## 🛠️ Prerequisites

### Required Software

1. **macOS** (required for iOS builds)
   - Monterey (12.x) or newer recommended
   - Xcode Command Line Tools

2. **Xcode 14.0+**
   - Download from Mac App Store
   - Or [developer.apple.com/xcode](https://developer.apple.com/xcode/)

3. **Unity 2022.3 LTS**
   - iOS Build Support module installed
   - Install via Unity Hub > Installs > Add Modules

4. **Apple Developer Account**
   - Free account: Device testing only
   - Paid account ($99/year): App Store distribution

### Unity Modules

Install these via Unity Hub:
- **iOS Build Support**
- **Xcode**

---

## 📦 iOS Build Configuration

### Using the iOS Build Config Tool

Open the iOS build configuration tool:
1. **Tools > Robot Tower Defense > iOS Build Configuration**
2. Configure settings in each tab
3. Click **Apply Settings** to save
4. Click **Build iOS Xcode Project** to generate build

### Build Settings Tab

#### App Identity
```
Bundle Identifier: com.yourstudio.robottowerdefense
Version: 1.0
Build Number: 1
```

> **Important:** Bundle identifier must be unique. Use reverse domain notation.

#### Code Signing
- **Automatic Signing** (recommended for beginners):
  - Unity handles provisioning profiles
  - Requires Apple Developer account credentials in Xcode

- **Manual Signing** (advanced):
  - Specify Team ID
  - Select provisioning profile type
  - Configure in Xcode after build

#### Device Compatibility
- **Target Device**: iPhone and iPad (universal)
- **Minimum iOS Version**: 13.0
  - iOS 13+ required for modern features
  - Supports iPhone 6S and newer

#### Orientation
Robot Tower Defense is designed for **Landscape**:
- ✅ Landscape Left
- ✅ Landscape Right
- ❌ Portrait (not supported)
- ❌ Portrait Upside Down

### App Icons Tab

iOS requires multiple icon sizes:
- **180x180** - iPhone @3x
- **120x120** - iPhone @2x
- **167x167** - iPad Pro
- **152x152** - iPad @2x
- **1024x1024** - App Store

Generate icons using tools like:
- [appicon.co](https://appicon.co/)
- [makeappicon.com](https://makeappicon.com/)

Import icons:
1. **Player Settings > iOS > Icon**
2. Drag images to appropriate slots
3. Click **Validate Icon Sizes** in tool

### Splash Screen Tab

iOS splash screen options:
1. **Launch Storyboard** (recommended for iOS 13+)
   - Adaptive for all devices
   - Configure in Player Settings > iOS > Splash Image

2. **Static Launch Images** (legacy)
   - Requires images for every device size
   - Only use if targeting iOS 12 or older

### Capabilities Tab

Configure in Xcode after building:
- **Game Center**: Leaderboards and achievements
- **In-App Purchase**: Shop monetization
- **Push Notifications**: (optional) Engagement

Privacy descriptions required if using:
- **Camera**: Not used (default text provided)
- **Microphone**: Not used (default text provided)
- **Location**: Not used (default text provided)

### Quick Actions Tab

Useful shortcuts:
- **Apply Recommended Settings**: Tower defense game presets
- **Increment Build Number**: For TestFlight/App Store uploads
- **Validate All Settings**: Check for missing required fields
- **Export Build Settings JSON**: Save configuration for CI/CD

---

## 🏗️ Building for iOS

### Step 1: Configure Build Settings

1. Open **iOS Build Configuration** tool
2. Set your **Bundle Identifier** (unique)
3. Set **Version** (e.g., 1.0)
4. Set **Build Number** (increment for each upload)
5. Click **Apply Recommended Settings**
6. Click **Apply Settings**

### Step 2: Generate Xcode Project

Two methods:

**Method A: Using Build Tool**
1. Click **Build iOS Xcode Project** in tool
2. Choose build location
3. Wait for build to complete

**Method B: Manual Build**
1. **File > Build Settings**
2. Switch platform to **iOS**
3. Click **Build**
4. Choose build location

### Step 3: Open in Xcode

1. Navigate to build location
2. Double-click `Unity-iPhone.xcodeproj`
3. Xcode will open the project

### Step 4: Configure Signing in Xcode

1. Select **Unity-iPhone** project
2. Select **Signing & Capabilities** tab
3. Check **Automatically manage signing**
4. Select your **Team** (Apple Developer account)
5. Xcode will generate provisioning profile

### Step 5: Build and Run

**For Device:**
1. Connect iPhone/iPad via USB
2. Select device in Xcode toolbar
3. Click **Run** (▶️) or `Cmd+R`
4. Trust developer certificate on device (Settings > General > Device Management)

**For Simulator:**
1. Select simulator in Xcode toolbar (e.g., iPhone 14)
2. Click **Run** (▶️)
3. Simulator will launch automatically

---

## 🎮 iOS-Specific Features

### Safe Area Support

The `iOSSafeAreaHandler` automatically adjusts UI for notches and home indicators.

**Auto-apply to Canvas:**
```csharp
// Automatically added to all Canvas objects at runtime
// No manual setup required
```

**Manual setup:**
```csharp
using RobotTowerDefense.UI;

public class MyCanvas : MonoBehaviour
{
    void Start()
    {
        var safeAreaHandler = gameObject.AddComponent<iOSSafeAreaHandler>();
        safeAreaHandler.SetRespectEdges(
            top: true,    // Respect notch
            bottom: true, // Respect home indicator
            left: true,   // Respect side insets
            right: true   // Respect side insets
        );
    }
}
```

**Inspector Configuration:**
- **Respect Top**: Avoid notch area
- **Respect Bottom**: Avoid home indicator
- **Respect Left/Right**: Avoid rounded corners
- **Show Debug Info**: Visualize safe area boundaries

### Touch Gestures

The `iOSInputHandler` provides iOS-optimized touch detection:

**Gesture Types:**
- **Tap**: Quick touch
- **Double Tap**: Two quick taps
- **Long Press**: Hold for 0.5s
- **Swipe**: Directional swipe (up/down/left/right)
- **Pinch**: Two-finger zoom (future camera control)

**Subscribe to gestures:**
```csharp
using RobotTowerDefense.Input;

void Start()
{
    iOSInputHandler.OnTap += HandleTap;
    iOSInputHandler.OnSwipe += HandleSwipe;
    iOSInputHandler.OnDoubleTap += HandleDoubleTap;
    iOSInputHandler.OnLongPress += HandleLongPress;
    iOSInputHandler.OnPinch += HandlePinch;
}

void HandleTap(Vector2 position)
{
    Debug.Log($"Tap at {position}");
    // Handle tower placement, UI interaction, etc.
}

void HandleSwipe(Vector2 direction)
{
    Debug.Log($"Swipe direction: {direction}");
    SwipeDirection dir = iOSInputHandler.GetSwipeDirection(direction);
    // Handle camera pan, menu swipe, etc.
}
```

**Convert screen to world position:**
```csharp
Vector2 touchPosition = touch.position;
Vector3 worldPosition = iOSInputHandler.Instance.ScreenToWorldPosition(touchPosition);
// Use worldPosition for tower placement
```

### Haptic Feedback

Provide tactile feedback for player actions:

**Built-in game events:**
```csharp
using RobotTowerDefense.Input;

// Tower placement
iOSInputHandler.Instance.PlayPlacementHaptic(success: true);  // Success vibration
iOSInputHandler.Instance.PlayPlacementHaptic(success: false); // Error vibration

// Wave complete
iOSInputHandler.Instance.PlayWaveCompleteHaptic();

// Game over
iOSInputHandler.Instance.PlayGameOverHaptic();

// Button press
iOSInputHandler.Instance.PlayButtonHaptic();
```

**Custom haptics:**
```csharp
using RobotTowerDefense.Input;

// Light feedback (UI, buttons)
iOSInputHandler.Instance.TriggerHapticFeedback(HapticFeedbackType.Light);

// Medium feedback (tower placement, enemy death)
iOSInputHandler.Instance.TriggerHapticFeedback(HapticFeedbackType.Medium);

// Heavy feedback (wave complete, boss death)
iOSInputHandler.Instance.TriggerHapticFeedback(HapticFeedbackType.Heavy);

// Success/Warning/Error (contextual feedback)
iOSInputHandler.Instance.TriggerHapticFeedback(HapticFeedbackType.Success);
```

**Enable/disable in settings:**
```csharp
// Settings UI toggle
iOSInputHandler.Instance.SetHapticsEnabled(enabled: true);

// Check state
bool enabled = iOSInputHandler.Instance.GetHapticsEnabled();
```

---

## 💰 Monetization on iOS

### Unity Ads Configuration

The `AdManager` automatically selects iOS-specific placements:

**Unity Ads Dashboard:**
1. Visit [dashboard.unity3d.com](https://dashboard.unity3d.com/)
2. Create iOS app
3. Copy **Game ID** (iOS)
4. Create placements:
   - `Interstitial_iOS`
   - `Rewarded_iOS`
   - `Banner_iOS`

**Configure in Unity:**
1. Select **AdManager** GameObject in scene
2. Set **iOS Game ID**
3. Set iOS placement IDs
4. Enable **Test Mode** for development

**Platform-specific behavior:**
- Automatically uses iOS Game ID on iOS devices
- Automatically uses iOS placement IDs
- Banner position defaults to top (avoids home indicator)

### In-App Purchases

The `IAPManager` works with App Store:

**App Store Connect Setup:**
1. Visit [appstoreconnect.apple.com](https://appstoreconnect.apple.com/)
2. Create app record
3. Configure In-App Purchases:
   - Create products matching IAPManager product IDs
   - Set prices in App Store Connect
   - Submit for review

**Product IDs must match:**
```csharp
// IAPManager product IDs
"gems_100"
"gems_500"
"gems_1200"
"gems_3000"
"credits_5000"
"credits_25000"
"powerup_bundle"
"starter_pack"
"remove_ads"
"tower_skin_gold"
"tower_skin_neon"
"map_pack_1"
"premium_pass_monthly"
```

**Testing IAP:**
1. Create **Sandbox Tester** in App Store Connect
2. Sign out of App Store on device
3. Build and run from Xcode
4. Purchase prompts for sandbox credentials
5. Enter sandbox tester email/password

**Restore Purchases:**
- Required by Apple App Store guidelines
- Already implemented in `IAPManager.RestorePurchases()`
- Button in ShopUI for users

---

## 🎮 Game Center Integration

Configure achievements and leaderboards:

### Setup in App Store Connect

1. **Features > Game Center**
2. Enable Game Center
3. Create leaderboards:
   - High Score
   - Endless Mode Best Wave
   - Total Enemies Defeated

4. Create achievements (match AchievementManager IDs)

### Enable in Xcode

1. **Signing & Capabilities** tab
2. Click **+ Capability**
3. Add **Game Center**
4. Build and run

### Testing

- Game Center automatically authenticates on device
- Use real Apple ID or sandbox tester
- Leaderboards/achievements sync via Game Center

---

## 🚀 Deploying to App Store

### TestFlight (Internal Testing)

1. **Archive Build in Xcode:**
   - Product > Archive
   - Wait for archive to complete
   - Organizer window opens

2. **Upload to App Store Connect:**
   - Click **Distribute App**
   - Select **App Store Connect**
   - Click **Upload**
   - Wait for processing (10-30 minutes)

3. **Configure TestFlight:**
   - Visit App Store Connect
   - Select app > TestFlight tab
   - Add internal testers (up to 100)
   - Testers receive email with download link

4. **External Testing:**
   - Add external testers (up to 10,000)
   - First build requires App Review (24-48 hours)
   - Subsequent builds auto-approved

### App Store Submission

1. **Prepare App Store Listing:**
   - Screenshots (6.5" iPhone, 12.9" iPad)
   - App icon (1024x1024)
   - Description, keywords, category
   - Privacy policy URL
   - Age rating questionnaire

2. **Submit for Review:**
   - App Store Connect > My Apps > [Your App]
   - Click **+ Version** (e.g., 1.0)
   - Upload screenshots and metadata
   - Select build from TestFlight
   - Submit for review

3. **App Review Process:**
   - Typically 24-48 hours
   - May request additional info
   - Approve or reject decision

4. **Release:**
   - Manual release (you control when)
   - Automatic release (goes live immediately)

### Pre-Submission Checklist

- [ ] Bundle ID configured correctly
- [ ] All required app icons present
- [ ] Launch screen configured
- [ ] Privacy descriptions accurate
- [ ] Game Center configured (if using)
- [ ] IAP products created in App Store Connect
- [ ] Sandbox IAP testing successful
- [ ] Unity Ads iOS placements created
- [ ] No crashes or major bugs
- [ ] Compliance with App Store guidelines
- [ ] Age rating appropriate
- [ ] Privacy policy published

---

## 🐛 Troubleshooting

### Build Errors

**"No provisioning profiles found"**
- Solution: Enable **Automatically manage signing** in Xcode
- Or create provisioning profile in Apple Developer dashboard

**"Code signing failed"**
- Solution: Check Apple Developer account is active ($99/year)
- Verify Team ID is correct
- Trust developer certificate on device

**"Unsupported Swift version"**
- Solution: Update Xcode to latest version
- Ensure Unity 2022.3 LTS is up to date

### Runtime Issues

**UI cutoff by notch**
- Solution: Add `iOSSafeAreaHandler` to Canvas
- Verify **Respect Top** is enabled

**Touch not detecting**
- Solution: Check Camera has Physics Raycaster component
- Verify UI elements have RaycastTarget enabled

**Ads not showing**
- Solution: Wait 24 hours after creating Unity Ads project
- Enable **Test Mode** for immediate testing
- Check iOS Game ID is correct

**IAP not working**
- Solution: Use Sandbox Tester credentials
- Sign out of App Store on device first
- Products must exist in App Store Connect

**Performance issues**
- Solution: Enable **Metal Editor Support**
- Set **Script Call Optimization** to Fast But No Exceptions
- Reduce **Target Frame Rate** to 30 FPS for battery saving

### App Store Rejection Reasons

**4.3 Design: Spam**
- Make game unique, not a clone
- Provide substantial content

**2.1 Performance: App Completeness**
- Remove placeholder content
- All advertised features must work
- No crashes or major bugs

**5.1.1 Privacy: Data Collection**
- Accurate privacy policy URL
- Declare data collection in App Privacy section
- Proper consent mechanisms

**Guideline 2.3.10: Accurate Metadata**
- Screenshots match actual gameplay
- Description doesn't exaggerate features
- Age rating appropriate

---

## 📊 Performance Optimization

### Recommended Settings

Apply via **Quick Actions > Apply Recommended Settings**:

```csharp
Target Device: iPhone and iPad
Minimum iOS Version: 13.0
Default Orientation: Landscape Left
Strip Engine Code: true
Script Call Optimization: Fast But No Exceptions
Metal Editor Support: true
Metal API Validation: false (production)
Target Frame Rate: 60 FPS (30 FPS in battery save mode)
```

### Graphics Optimization

**Metal API (iOS graphics):**
- Only graphics API supported on iOS
- Hardware-accelerated rendering
- Excellent performance on A9+ chips

**Quality Settings:**
- Create iOS-specific quality preset
- Reduce shadow distance on older devices
- Use simple shaders for mobile

**Profiling:**
- Use Xcode Instruments
- Monitor frame rate, memory, battery
- Optimize hotspots identified in profiler

### Memory Management

**Texture Compression:**
- Use ASTC format (iOS native)
- Configure in Import Settings

**Object Pooling:**
- Already implemented in `ObjectPooler`
- Reuses enemies, projectiles, VFX

**Asset Bundles:**
- Consider for large map packs
- Reduce initial download size

---

## 📁 File Structure

iOS-specific files added:

```
Assets/
├── Scripts/
│   ├── Editor/
│   │   └── iOSBuildConfig.cs       # iOS build configuration tool
│   ├── Input/
│   │   └── iOSInputHandler.cs      # Touch gestures and haptics
│   └── UI/
│       └── iOSSafeAreaHandler.cs   # Notch/safe area support
```

---

## 🔗 Useful Resources

### Apple Developer

- **Apple Developer Portal**: [developer.apple.com](https://developer.apple.com/)
- **App Store Connect**: [appstoreconnect.apple.com](https://appstoreconnect.apple.com/)
- **App Store Review Guidelines**: [developer.apple.com/app-store/review/guidelines/](https://developer.apple.com/app-store/review/guidelines/)
- **Human Interface Guidelines**: [developer.apple.com/design/human-interface-guidelines/ios](https://developer.apple.com/design/human-interface-guidelines/ios)

### Unity Documentation

- **iOS Build Settings**: [docs.unity3d.com/Manual/ios-BuildSettings.html](https://docs.unity3d.com/Manual/ios-BuildSettings.html)
- **iOS Publishing**: [docs.unity3d.com/Manual/PublishingForiOS.html](https://docs.unity3d.com/Manual/PublishingForiOS.html)
- **Unity Ads iOS**: [docs.unity.com/ads/en-us/manual/iOSIntegration](https://docs.unity.com/ads/en-us/manual/iOSIntegration)
- **Unity IAP iOS**: [docs.unity3d.com/Manual/UnityIAPAppleConfiguration.html](https://docs.unity3d.com/Manual/UnityIAPAppleConfiguration.html)

### Tools

- **TestFlight**: [testflight.apple.com](https://testflight.apple.com/)
- **App Icon Generator**: [appicon.co](https://appicon.co/)
- **Screenshot Designer**: [app-mockup.com](https://app-mockup.com/)
- **ASO Tools**: [www.sensortower.com](https://www.sensortower.com/)

---

## ✅ Quick Reference

### Build Command Line (CI/CD)

```bash
# Unity command line build for iOS
/Applications/Unity/Hub/Editor/2022.3.0f1/Unity.app/Contents/MacOS/Unity \
  -quit \
  -batchmode \
  -projectPath /path/to/project \
  -executeMethod BuildScript.BuildiOS \
  -logFile build.log
```

### Common Terminal Commands

```bash
# Open Xcode project
open Unity-iPhone.xcodeproj

# List connected devices
xcrun xctrace list devices

# Check code signing
codesign -dv --verbose=4 build/Payload/RobotTowerDefense.app

# Archive for App Store
xcodebuild archive -scheme Unity-iPhone -archivePath build/RobotTD.xcarchive
```

---

**iOS platform support added in Version 1.7** ✅
