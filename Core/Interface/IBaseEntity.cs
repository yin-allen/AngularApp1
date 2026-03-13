namespace Core.Interface
{
    public interface IBaseEntity
    {
        int Id { get; set; }

        /// <summary>
        /// 自動建立新增時間，只在第一次新增時on值
        /// </summary>
        DateTime? CreateTime { get; set; }

        /// <summary>
        /// 自動更新更新間
        /// 假設我們用parent.Childs.Add(new Chind())，然後使用grParent.Update(parent)去新增這個child，則parent.UpdateTime是會被更新的
        /// </summary>
        DateTime? UpdateTime { get; set; }
    }
}