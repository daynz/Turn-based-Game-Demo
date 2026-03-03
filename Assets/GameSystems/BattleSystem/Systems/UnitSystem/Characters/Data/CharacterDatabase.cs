using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BH.Framework.Services;
using BH.GameSystems.BattleSystem.Interfaces;
using BH.GameSystems.BattleSystem.Systems.UnitSystem.Base;
using Newtonsoft.Json;
using UnityEngine;

namespace BH.GameSystems.BattleSystem.Systems.UnitSystem.Characters.Data
{
    /// <summary>
    /// 角色专用数据库
    /// </summary>
    [Serializable]
    public class CharacterDatabase : UnitDatabase<CharacterData>, ICharacterDatabase
    {
        ICharacterData IUnitDatabase<ICharacterData>.GetUnit(string id)
        {
            return GetUnit(id);
        }

        bool IUnitDatabase<ICharacterData>.TryGetUnit(string id, out ICharacterData unit)
        {
            var result = TryGetUnit(id, out var data);
            unit = data;
            return result;
        }

        //TODO:修改基类
        public async Task LoadFromJson()
        {
            try
            {
                // var textAsset = await ResourceService.Instance.LoadPersistentAssetAsync<TextAsset>("Characters");
                // if (textAsset)
                // {
                //     // if (string.IsNullOrEmpty(relativePath))
                //     // {
                //     //     LogManager.Instance.Warning($"[CharacterDatabase] {relativePath} 路径不存在，跳过加载。");
                //     //     return;
                //     // }
                //
                //     //var fullPath = Path.Combine(Application.streamingAssetsPath, relativePath);
                //     //var json = File.ReadAllText(fullPath);
                //     //var items = JsonConvert.DeserializeObject<List<CharacterData>>(json);
                //     // if (items == null)
                //     // {
                //     //     //LogManager.Instance.Warning($"[CharacterDatabase] 无法解析JSON文件：{fullPath}");
                //     //     return;
                //     // }
                //
                //     // foreach (var data in items)
                //     // {
                //     //     units.Add(data);
                //     // }
                //
                //     //base.LoadUnitDatabaseFromJson(relativePath);
                //
                //     //LogManager.Instance.Info($"[CharacterDatabase] 成功从 {fullPath} 加载数据，共 {items.Count} 个。");
                //
                //     var json = textAsset.text;
                //     try
                //     {
                //         var items = JsonConvert.DeserializeObject<List<CharacterData>>(json);
                //         if (items == null)
                //         {
                //             LOGService.Warning($"无法解析JSON文件", Name);
                //             return;
                //         }
                //
                //         foreach (var data in items)
                //         {
                //             units.Add(data);
                //         }
                //     }
                //     catch (Exception e)
                //     {
                //         Console.WriteLine(e);
                //         throw;
                //     }
                // }
            }
            catch (FileNotFoundException)
            {
                //LogManager.Instance.Error($"[CharacterDatabase] 文件未找到：{fullPath}");
            }
            catch (JsonException ex)
            {
                LOGService.Error($"[CharacterDatabase] JSON格式错误：{ex.Message}", Name);
            }
            catch (Exception ex)
            {
                LOGService.Error($"[CharacterDatabase] 加载数据时发生未知错误：{ex.Message}", Name);
            }
        }
    }
}