# Project Map

`Assets/_Project/Scripts/UI/ActCardUI.cs` - Component điều khiển thẻ Hồi - class `ActCardUI`, Setup, OnPointerDown, OnPointerUp, PlayIntroAnimation
`Assets/_Project/Scripts/UI/ActSelectPreview.cs` - Khởi tạo dữ liệu mẫu và điều hướng màn chọn Hồi - class `ActSelectPreview`, SetupPreviewData, AutoFindReferences, SetupNavigation
`Assets/_Project/Scripts/UI/BackgroundTwinkleStars.cs` - Hiệu ứng lấp lánh ngẫu nhiên cho các chấm sao nền - class `BackgroundTwinkleStars`, TwinkleRoutine
`Assets/_Project/Editor/GenerateUISprites.cs` - Menu sinh 8 sprite 9-slice bo góc và gradient - class `GenerateUISprites`, GenerateAllSprites
`Assets/_Project/Editor/DiagnoseActSelectView.cs` - Tool chẩn đoán trực quan toàn bộ layout và trạng thái ActSelectView - class `DiagnoseActSelectView`, RunDiagnosis
`Assets/_Project/Editor/FixActSelectView.cs` - Tool tự động chuẩn hóa cấu hình ActSelectView - class `FixActSelectView`, FixAll
`Assets/_Project/Scripts/UI/LevelRewards.cs` - Công thức tính thưởng sao & kim cương theo mốc - class `LevelRewards`, GetStarReward, GetDiamondReward, GetConditionText
`Assets/_Project/Scripts/UI/ILevelDataProvider.cs` - Interface và struct dữ liệu các node trong Hồi - interface `ILevelDataProvider`, struct `LevelNodeData`, enum `NodeState`
`Assets/_Project/Scripts/UI/MockLevelDataProvider.cs` - Cung cấp dữ liệu mẫu cho bản đồ 3 Hồi - class `MockLevelDataProvider`, GetLevelsForAct, GetActStars
`Assets/_Project/Scripts/UI/PathDrawer.cs` - Vẽ đường nét đứt cong Bezier giữa các node - class `PathDrawer`, RebuildPath, SetNodes
`Assets/_Project/Scripts/UI/LevelNodeUI.cs` - Điều khiển node thường và Boss, nhấp nháy, rung lắc - class `LevelNodeUI`, Setup, PlayEntranceAnimation, PlayShakeAnimation
`Assets/_Project/Scripts/UI/LevelDetailModal.cs` - Popup chi tiết màn, mốc quà, chọn lân, nút chơi - class `LevelDetailModal`, Open, Close
`Assets/_Project/Scripts/UI/RewardRowUI.cs` - Component quản lý hiển thị từng hàng mốc thưởng trong modal chi tiết màn - class `RewardRowUI`, Setup, AutoBind
`Assets/_Project/Scripts/UI/LevelDetailView.cs` - Màn bản đồ chọn màn, header tiến độ, toast - class `LevelDetailView`, Initialize, ShowToast
`Assets/_Project/Scripts/UI/LevelSelectFlow.cs` - Điều phối chuyển cảnh Chọn Hồi <-> Chọn Màn, phím Escape - class `LevelSelectFlow`, TransitionToLevelDetail, TransitionToActSelect
`Assets/_Project/Editor/BuildLevelDetailView.cs` - Tool dựng prefab LevelNode, BossNode và lắp ráp LevelDetailView - class `BuildLevelDetailView`, BuildAll
`Assets/_Project/Scripts/Data/LevelData.cs` - ScriptableObject cấu hình từng màn chơi (màn thường & Boss) - class `LevelData`, GetStarReward, GetDiamondReward
`Assets/_Project/Scripts/Data/ActData.cs` - ScriptableObject cấu hình thông tin từng Hồi - class `ActData`, LevelCount
`Assets/_Project/Scripts/Data/LevelDatabase.cs` - ScriptableObject danh mục tập trung tất cả Hồi và Màn - class `LevelDatabase`, GetLevel, GetAct, GetActOfLevel
`Assets/_Project/Scripts/Data/ProgressStorage.cs` - Trừu tượng hóa lưu trữ tiến độ và phát thưởng - interface `IProgressStorage`, `IRewardSink`, `PlayerPrefsProgressStorage`, `EconomyRewardSink`
`Assets/_Project/Scripts/Data/LevelProgressManager.cs` - Quản lý tiến độ chơi, bitmask nhận thưởng, mở khóa màn/Hồi - class `LevelProgressManager`, ReportLevelResult, GetStars, IsMilestoneClaimed
`Assets/_Project/Scripts/Data/ProgressLevelDataProvider.cs` - Cầu nối cung cấp dữ liệu thực từ LevelProgressManager cho giao diện - class `ProgressLevelDataProvider`
`Assets/_Project/Scripts/Data/LevelRules.cs` - Tính toán luật chơi và chỉ số phần trăm máu gộp mạng - class `LevelRules`, CalcHpRemaining01, MaxHearts
`Assets/_Project/Editor/SetupLevelData.cs` - Tool tự động khởi tạo 15 LevelData, 3 ActData và LevelDatabase - class `SetupLevelData`, SetupAll
`Assets/_Project/Editor/LevelProgressSelfTest.cs` - Bộ 26 bài kiểm tra tự động độ chính xác của tầng tiến độ - class `LevelProgressSelfTest`, RunAllTests
`Assets/_Project/Editor/LevelProgressDebugWindow.cs` - Cửa sổ Editor kiểm tra và can thiệp tiến độ - class `LevelProgressDebugWindow`
`Assets/_Project/Scripts/Data/LanCatalog.cs` - Cung cấp danh mục nhân vật Lân cho toàn game và lấy Lân đang chọn - class `LanCatalog`, GetAllLans, GetSelectedLan
`Assets/_Project/Scripts/UI/LevelDataService.cs` - Điểm truy cập duy nhất cung cấp ILevelDataProvider cho toàn bộ UI - class `LevelDataService`, Provider, SetCustomProvider
`Assets/_Project/Scripts/UI/ActSelectView.cs` - Điều khiển màn Chọn Hồi, cập nhật động thẻ Hồi, TopBar tiền tệ, toast - class `ActSelectView`, Initialize, RefreshDisplay, ShowToast
`Assets/_Project/Editor/ValidateLevelSelectBindings.cs` - Tool kiểm tra tính toàn vẹn của tất cả liên kết trong Scene LevelSelect - class `ValidateLevelSelectBindings`, ValidateAll
`Assets/_Project/Editor/FixLanPrefabMismatch.cs` - Tool tự động chuẩn hóa tham chiếu LanData/lanConfig trên 4 prefab Lân - class `FixLanPrefabMismatch`
`Assets/_Project/Scripts/Gameplay/GameplayBalance.cs` - Gom tất cả hằng số cân bằng Gameplay giai đoạn 5A theo Excel - class `GameplayBalance`
`Assets/_Project/Editor/FindNullSprites.cs` - Tool Editor quét và tự động sửa các Image sprite null trong Scene và UI Prefab - class `FindNullSprites`, ScanAndFix
`Assets/_Project/Editor/PlaytestRunner.cs` - Tool chạy kiểm thử tự động 5 kịch bản kết thúc Gameplay - class `PlaytestRunner`, RunAllPlaytests
Assets/_Project/Scripts/Gameplay/LevelSession.cs - Quản lý trạng thái phiên chơi Gameplay và tính toán LanStats - class LevelSession, LanStats, Initialize, Reset, AwardStars
Assets/_Project/Scripts/Gameplay/LevelSessionBootstrap.cs - Khởi tạo phiên thử nghiệm khi chạy trực tiếp Scene Gameplay - class LevelSessionBootstrap
Assets/_Project/Scripts/Gameplay/BackgroundManager.cs - Đổi nền chiến đấu theo Hồi, phủ tối và cover camera - class BackgroundManager, ApplyActBackground, UpdateScale
Assets/_Project/Scripts/Gameplay/GameplayDebugPanel.cs - Panel F9 debug Gameplay (thắng/thua/máu/hồi sinh) - class GameplayDebugPanel
Assets/_Project/Editor/Phase5AGameplaySetup.cs - Tool tự động cấu hình Build Settings, waves, trùm, HUD và panel cho Gameplay - class Phase5AGameplaySetup
Assets/_Project/Editor/Phase5AVerificationRunner.cs - Bộ kiểm thử 11 ca tự động xác minh toàn diện giai đoạn 5A - class Phase5AVerificationRunner, RunAllTests

Assets/_Project/Editor/ExecuteAllUserScenarios.cs - Bộ kiểm thử 7 kịch bản Gameplay giai đoạn 5A.3 (vòng đời hồi sinh, bỏ cuộc, cấm hồi sinh lần 2, VictoryPanel fail-safe, layout HUD, mảnh vỡ đèn gai, chơi lại nhiều lần) - class ExecuteAllUserScenarios
Assets/_Project/Editor/VerifyHUDPhase5A2.cs - Công cụ kiểm tra bố cục HUD Gameplay 5A.2 (chân dung, SafeArea, HPSlider, Lives, Settings, StarsChip, responsive 9:16, 9:20, 3:4) - class VerifyHUDPhase5A2
Assets/_Project/Scripts/Gameplay/SpikedLanternDebris.cs - Mảnh vỡ đèn gai bay tỏa 8 hướng, kéo vệt cam đỏ, gây sát thương va chạm và tự hủy ngoài màn hình - class SpikedLanternDebris
Assets/_Project/Scripts/Gameplay/GameplayScaleConfig.cs - ScriptableObject cấu hình tỷ lệ chiều rộng chuẩn trên màn hình và tỉ lệ collider - class GameplayScaleConfig
Assets/_Project/Editor/GameplayScaleTool.cs - Tool đo kích thước bounding box và áp dụng scale chuẩn 100% cho 12 prefabs - class GameplayScaleTool
Assets/_Project/Scripts/UI/BossHUD.cs - Thanh máu Boss độc lập dưới chip mục tiêu, fill đỏ #FF5A5F, 2 vạch phase 66%/33% - class BossHUD
Assets/_Project/Scripts/Data/SkillConfig.cs - ScriptableObject cấu hình chi tiết 4 kỹ năng Lân (cooldown, duration, damage, bossDamageCap, VFX parameters) - class SkillConfig
Assets/_Project/Editor/GenerateVFXSprites.cs - Tool Editor sinh 8 sprite PNGs trắng trong suốt và Material VFX additive - class GenerateVFXSprites, GenerateAllSprites
Assets/_Project/Editor/SetupSkillData.cs - Tool tự động khởi tạo 4 asset SkillConfig theo GDD và liên kết vào LanData - class SetupSkillData, SetupAll
Assets/_Project/Scripts/VFX/SkillVFXPool.cs - Object pool quản lý hạt Particle Systems và Sprite hiệu ứng chiêu thức, tự động dọn sạch khi kết thúc trận - class SkillVFXPool
Assets/_Project/Scripts/Gameplay/PlayerSkillController.cs - Điều khiển vòng đời, kích hoạt 4 chiêu thức Lân, camera punch, bullet-time, tính damage, dọn đạn và in telemetry - class PlayerSkillController
Assets/_Project/Editor/ApplySkillUIBindings.cs - Tool tự động dựng cấu trúc nút Skill tròn 132px, Banner, ScreenBorderFlash vào scene Gameplay - class ApplySkillUIBindings
Assets/_Project/Editor/VerifySkillPhase5A4.cs - Bộ kiểm thử tự động toàn diện 6 mục cho Kỹ năng và Nút Skill giai đoạn 5A.4 - class VerifySkillPhase5A4

Assets/_Project/Art/VFX/Shaders/VFX_ScrollMask.shader - Shader cuộn UV và feather mask hỗ trợ URP 2D, URP Forward và Built-in fallback
Assets/_Project/Scripts/Data/VfxImportConfig.cs - ScriptableObject cấu hình quy tắc import, crop padding, pivot và chuyển đổi grayscale/alpha cho 11 sprite vẽ tay
Assets/_Project/Editor/VfxImportHelper.cs - Tool Editor tự động import, cắt xén, tính pivot, sinh bản xám/alpha, thiết lập 4 Materials VFX
Assets/_Project/Scripts/VFX/JuiceFx.cs - Hệ thống phản hồi thị giác & nhịp điệu realtime (HitStop, CameraShake có decay, ScreenFlash, VignettePulse, EnemyHitReaction)
Assets/_Project/Scripts/VFX/VfxLayer.cs - Điều khiển lớp thị giác độc lập bằng AnimationCurve (scale, alpha, rotation, position, scroll)
Assets/_Project/Editor/CreateSkillVFXPrefabs.cs - Tool Editor sinh 4 Prefabs VFX tại Assets/_Project/Prefabs/VFX/ (Kim, Hỏa, Thủy, Vàng)
Assets/_Project/Editor/VerifySkillPhase5A5.cs - Bộ kiểm thử tự động toàn diện 7 mục xác minh hình ảnh Skill vẽ tay, 4 nhịp chuyển động và phản hồi 5A.5
Assets/_Project/Scripts/VFX/SkillVfxLifetimeGuard.cs - Bảo vệ vòng đời hiệu ứng thị giác, tự động dọn sạch mọi VFX sau duration + 1.5s - class SkillVfxLifetimeGuard
Assets/_Project/Editor/VerifySkillPhase5A6.cs - Bộ kiểm thử tự động 7 hạng mục giai đoạn 5A.6 (VFX alpha, kim tuyến, pháo hoa, chip Boss, đồng bộ máu Hangar/Gameplay, popup LevelDetailModal, Banner Skill) - class VerifySkillPhase5A6
`Assets/_Project/Scripts/Data/SceneNames.cs` - Quản lý tên Scene tập trung và log điều hướng [NAV] - class `SceneNames`, LoadScene, IsValidScene
`Assets/_Project/Editor/ValidateSceneNames.cs` - Kiểm tra và sửa Build Settings, bind reference Scene bằng Unity API - class `ValidateSceneNames`, ValidateAndFixSceneNames
`Assets/_Project/Editor/TestSceneNavigation.cs` - Bộ kiểm thử chuyển cảnh tự động (Hangar, LevelSelect, Gameplay) - class `TestSceneNavigation`, RunAllNavigationTests

`Assets/_Project/Scripts/Data/LanDimensions.cs` - TA-nh toAn vA quy ` i kA-ch th>c %W, %H sang World Units theo Camera hin ti - class `LanDimensions`
`Assets/_Project/Editor/MeasureLanTool.cs` - CA'ng c `o b? rTng L c a 4 LAn vA thit l-p 2 `im neo MouthPoint, CenterPoint - class `MeasureLanTool`
`Assets/_Project/Editor/VerifySkillPhase5A7.cs` - BT kim th- t `Tng 6 tiAu chA- Giai `on 5A.7 cho 4 skill chucn GDD - class `VerifySkillPhase5A7`

`Assets/_Project/Scripts/VFX/VfxSizeGuard.cs` - Giám sát và tự động kẹp scale AABB của hiệu ứng skill không vượt quá trần cấu hình (%W, %H, %Area) - class `VfxSizeGuard`
`Assets/_Project/Editor/VerifySkillPhase5A8.cs` - Bộ kiểm thử tự động 9 tiêu chí Giai đoạn 5A.8 (Node Boss, trần kích thước, 4 skill GDD, Banner, Cài Đặt giữa trận, Captures) - class `VerifySkillPhase5A8`

77: `Assets/_Project/Editor/VerifySkillVfxFix.cs` - Bộ kiểm thử tự động 3 tiêu chí xác minh bộ đếm cooldown, khử nền đen Thủy Lân và hiệu ứng phun lửa Hỏa Lân - class `VerifySkillVfxFix`, RunVerificationAssets/_Project/Scripts/UI/UiButtonStyleSet.cs - ScriptableObject cấu hình 3 bộ kiểu nút GoldPrimary, NavySecondary, CoralExit - class UiButtonStyleSet, ButtonStyleData, enum ButtonStyleType
Assets/_Project/Scripts/UI/StyledButton.cs - Component dùng chung điều khiển giao diện, layout, hiệu ứng bấm và trạng thái disabled của nút - class StyledButton
Assets/_Project/Scripts/UI/LorePanelUI.cs - Điều khiển toàn diện bảng Truyền Thuyết Hangar (thẻ giấy kem, huy chương, drop-cap, gradient fade, cuộn, chip kỹ năng) - class LorePanelUI, Show, Hide
Assets/_Project/Editor/ButtonAssetTools.cs - Menu Tool import bộ nút 9-slice chuẩn và xử lý nền ảnh - class ButtonAssetTools, ImportButtonAssets, RemoveWhiteBackground
Assets/_Project/Editor/SetupUIPhase1.cs - Tool tự động thiết lập bộ nút mới và bảng truyền thuyết lên cả 3 Scene và Prefab - class SetupUIPhase1, RunAllSetup
Assets/_Project/Editor/VerifyUIPhase1.cs - Bộ kiểm thử tự động 7 tiêu chí xác minh UI-1 và xuất 12 ảnh minh chứng - class VerifyUIPhase1, RunAllVerifications
