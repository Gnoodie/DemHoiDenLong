using UnityEditor;
using UnityEngine;
using DemHoiDenLong.Data;
using System.IO;

public class SetupLanDataGDD : EditorWindow
{
    [MenuItem("DemHoiDenLong/Setup LanData from GDD")]
    public static void ApplyGDDData()
    {
        string[] paths = {
            "Assets/_Project/Data/Lans/1.asset",
            "Assets/_Project/Data/Lans/2.asset",
            "Assets/_Project/Data/Lans/3.asset",
            "Assets/_Project/Data/Lans/4.asset"
        };

        Color color1, color2, color3, color4;
        ColorUtility.TryParseHtmlString("#F5D76E", out color1); // Kim Lan
        ColorUtility.TryParseHtmlString("#FF6B4A", out color2); // Hoa Lan
        ColorUtility.TryParseHtmlString("#7FE8F5", out color3); // Thuy Lan
        ColorUtility.TryParseHtmlString("#F5C518", out color4); // Lan Vang

        // 1: Kim Lan
        UpdateAsset(paths[0], "1", "Kim Lân", "Lân Nhỏ giữ trống", true, 0, -1, 100, 300, 5, 5, 
            "Lướt nhanh bất tử", 0, 15,
            "Lân lướt nhanh về phía trước trong 1.5 giây, trong thời gian đó không nhận sát thương từ bất kỳ nguồn nào.",
            "Kim Lân là hoá thân đầu tiên và trẻ nhất trong Tứ Lân, sinh ra từ tiếng trống hội đầu tiên vang lên ở sân đình làng — nơi những đứa trẻ tập múa lân mỗi mùa Trung Thu. Vì thế Kim Lân mang hình hài thân thiện, chưa hoàn thiện sức mạnh nguyên tố như ba người anh em, nhưng bù lại sở hữu sự lanh lợi và lòng dũng cảm của một đứa trẻ chưa từng biết sợ bóng tối.\n\nKim Lân không chọn phe mạnh nhất hay nguyên tố dữ dội nhất — cậu chọn ở lại tuyến đầu, ru ngủ dân làng bằng ánh sáng dịu và giữ nhịp trống hội không bao giờ tắt. Kỹ năng \"Lướt nhanh bất tử\" chính là bản năng của một chú lân nhỏ lao mình che chắn cho đoàn rước đèn, bất chấp hiểm nguy, dù chỉ trong khoảnh khắc.",
            color1, 1.00f);

        // 2: Hoa Lan
        UpdateAsset(paths[1], "2", "Hỏa Lân", "Lân Đỏ đốt đêm", false, 500, -1, 90, 200, 12, 3,
            "Phun lửa quét ngang", 50, 20,
            "Phun một dải lửa rộng theo chiều ngang màn hình, gây sát thương liên tục lên mọi kẻ địch trong vùng lửa.",
            "Khi lũ Đèn Tàn tràn vào khu Phố Cổ — nơi hàng ngàn lồng đèn giấy treo dọc mái ngói và phố nhỏ — chính là Hỏa Lân xuất hiện đầu tiên, sinh ra từ ngọn lửa của chiếc đèn kéo quân cháy mãi không tắt suốt trăm năm trong một cửa hiệu cổ. Hỏa Lân mang tính cách nóng nảy, bốc đồng, không thích chờ đợi hay phòng thủ — với cậu, cách bảo vệ ánh sáng tốt nhất là thiêu rụi bóng tối trước khi nó kịp lan ra.\n\nSự liều lĩnh ấy khiến Hỏa Lân từng bị Nguyên Lân trách mắng vì đã đốt cháy vài dãy phố trong lúc chiến đấu, nhưng cũng chính ngọn lửa không khoan nhượng đó đã cứu Phố Cổ khỏi đêm tối nhiều lần. Kỹ năng \"Phun lửa quét ngang\" là biểu hiện trực tiếp của bản tính ấy: tấn công diện rộng, sát thương cao, không màng hậu quả.",
            color2, 1.20f);

        // 3: Thuy Lan
        UpdateAsset(paths[2], "3", "Thủy Lân", "Lân Xanh trông sông", false, 800, -1, 110, 220, 8, 4,
            "Sóng nước đẩy lùi địch", 30, 18,
            "Tạo ra một làn sóng nước mạnh xô đẩy toàn bộ kẻ địch trên màn hình về phía sau, làm chậm chúng 50% trong 3 giây.",
            "Thủy Lân sinh ra từ khúc sông chảy quanh làng, nơi người dân thả đèn hoa đăng mỗi đêm rằm để tưởng nhớ tổ tiên. Không giống Hỏa Lân nóng nảy, Thủy Lân điềm tĩnh, kiên nhẫn và mang trong mình ký tự của hàng ngàn ngọn đèn hoa đăng đã trôi qua tay mình — vì thế Thủy Lân chiến đấu không phải để tiêu diệt, mà để \"đẩy lùi\", nhắc nhở lũ Đèn Tàn nhớ lại chúng từng là những ngọn đèn mang ước nguyện của con người trước khi bị bỏ quên.\n\nThủy Lân đặc biệt mạnh khi kẻ địch đông đảo — bản tính của cậu là làm chậm lại, xoa dịu cả một vùng hỗn loạn thay vì đối đầu trực diện từng kẻ một. Kỹ năng \"Sóng nước đẩy lùi địch\" chính là một lời nhắc nhở dịu dàng nhưng đủ mạnh để cả bầy quái vật phải chững lại.",
            color3, 1.13f);

        // 4: Lan Vang
        UpdateAsset(paths[3], "4", "Lân Vàng", "Lân Ánh Trăng", false, -1, 300, 150, 250, 15, 5,
            "Mưa pháo hoa toàn màn", 100, 25,
            "Bắn ra mưa pháo hoa đa hướng bao phủ toàn màn hình, mỗi đầu đạn pháo hoa gây sát thương lớn khi chạm địch.",
            "Lân Vàng không sinh ra ở nhân gian như ba người anh em — đây là hoá thân cuối cùng và mạnh nhất của Nguyên Lân, được tôi luyện ngay tại Cung Trăng dưới ánh sáng của Hằng Nga và sự bầu bạn của Thỏ Ngọc. Trong khi Kim, Hỏa, Thủy Lân phải chiến đấu để bảo vệ từng vùng đất nhân gian, Lân Vàng mang sứ mệnh lớn hơn: trấn giữ cánh cổng cuối cùng nối Cung Trăng với hạ giới, ngăn không cho bóng tối vượt qua con đường ánh trăng vào đêm hội.\n\nSự uy nghi của Lân Vàng thể hiện qua kỹ năng \"Mưa pháo hoa toàn màn\" — không phải một đòn tấn công đơn thuần, mà là lời chúc phúc của cả Cung Trăng gửi xuống nhân gian, mỗi tia pháo hoa mang theo một điều ước của người trần đã được Hằng Nga chấp thuận. Đây cũng là lý do Lân Vàng chỉ xuất hiện với người chơi thông qua nghi thức đặc biệt — như một ân huệ hiếm hoi chứ không phải sức mạnh có thể đạt được bằng nỗ lực thông thường.",
            color4, 1.73f);
            
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("GDD Lan Data Applied Successfully!");
    }

    private static void UpdateAsset(string path, string id, string name, string nick, bool defaultUnlock, int costStar, int costGem, float hp, float speed, float dmg, float fireRate, string skillName, float skillDmg, float skillCd, string skillDesc, string lore, Color mainColor, float upgradeMultiplier)
    {
        LanData lan = AssetDatabase.LoadAssetAtPath<LanData>(path);
        if (lan == null)
        {
            lan = ScriptableObject.CreateInstance<LanData>();
            AssetDatabase.CreateAsset(lan, path);
        }

        lan.lanId = id;
        lan.lanName = name;
        lan.nickname = nick;
        lan.isUnlockedByDefault = defaultUnlock;
        lan.unlockCostStars = costStar;
        lan.unlockCostGems = costGem;
        lan.maxHp = hp;
        lan.moveSpeed = speed;
        lan.baseDamage = dmg;
        lan.fireRate = fireRate;
        lan.skillName = skillName;
        lan.skillDamage = skillDmg;
        lan.skillCooldown = skillCd;
        lan.skillDescription = skillDesc;
        lan.storyDescription = lore;
        lan.titleColor = mainColor;
        lan.skillTitleColor = mainColor;
        lan.upgradeCostMultiplier = upgradeMultiplier;

        EditorUtility.SetDirty(lan);

        EditorUtility.SetDirty(lan);
    }
}
