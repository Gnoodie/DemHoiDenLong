using System.Collections.Generic;
using UnityEngine;

namespace DemHoiDenLong.Data
{
    /// <summary>
    /// ScriptableObject chứa toàn bộ cốt truyện chung của Đêm Hội Đèn Lồng.
    /// Gán một instance duy nhất vào LoreUI hoặc GameManager để dùng xuyên suốt.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_NarrativeData", menuName = "DemHoiDenLong/Narrative Data")]
    public class NarrativeData : ScriptableObject
    {
        [Header("Tiêu đề câu chuyện lớn")]
        public string mainTitle = "CỐT TRUYỆN TỨ LÂN — ĐÊM HỘI ĐÈN LỒNG";

        [Header("Khởi nguyên chung (Prologue)")]
        [TextArea(5, 20)]
        public string prologueTitle = "Tứ Lân Hộ Đăng";

        [TextArea(5, 20)]
        public string prologueBody =
            "Tương truyền, mỗi năm vào đêm rằm tháng Tám, ánh trăng tròn nhất sẽ hé mở một con đường " +
            "ánh sáng nối làng quê với Cung Trăng, để muôn dân được đón lộc của Hằng Nga và Chú Cuội. " +
            "Nhưng bóng tối luôn rình rập nuốt chửng con đường ấy — lũ Đèn Tàn, những chiếc lồng đèn bị " +
            "bỏ quên, tắt lụi và hoá quái, cùng bè lũ cá chép đêm chuyên đi dập tắt ánh sáng lễ hội.\n\n" +
            "Để bảo vệ Đêm Hội, một linh vật lân cổ xưa mang tên Nguyên Lân đã tự tách linh hồn mình " +
            "thành bốn hoá thân, mỗi hoá thân mang một nguyên tố và một sứ mệnh riêng, cùng nhau thắp lại " +
            "con đường ánh sáng qua ba vùng đất: Làng Quê, Phố Cổ, và Cung Trăng.";

        [Header("Hồi kết (Epilogue / Thông điệp)")]
        [TextArea(5, 20)]
        public string epilogueTitle = "Thắp Lại Ngọn Đèn";

        [TextArea(5, 20)]
        public string epilogueBody =
            "Đêm Hội Đèn Lồng chiến thắng không phải bằng cách xoá sổ bóng tối, " +
            "mà bằng cách thắp lại từng ngọn đèn đã tắt.\n\n" +
            "Lũ Đèn Tàn thực chất là những ước nguyện bị lãng quên — không phải kẻ thù để tiêu diệt, " +
            "mà là nỗi buồn cần được nhớ đến. Và Thỏ Ngọc, người bạn thân nhất của Lân Vàng, " +
            "chỉ cần được nghe lại tên mình một lần nữa để thoát khỏi bóng tối.";

        [Header("Danh sách các hồi (Acts) — tham chiếu đến LanConfig")]
        public List<ActData> acts;
    }

    [System.Serializable]
    public class ActData
    {
        [Tooltip("Tên hồi, ví dụ: HỒI 01 — LÀNG QUÊ")]
        public string actName;

        [Tooltip("Mô tả ngắn về bối cảnh và xung đột chính của hồi này")]
        [TextArea(3, 8)]
        public string actSummary;

        [Tooltip("Nhân vật Lân đại diện cho hồi này")]
        public LanConfig representativeLan;

        [Tooltip("Tên boss cuối hồi")]
        public string bossName;

        [TextArea(2, 5)]
        public string bossLore;
    }
}
