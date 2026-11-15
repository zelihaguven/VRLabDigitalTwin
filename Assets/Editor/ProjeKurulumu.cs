// Assets/Editor/ProjeKurulumu.cs
// Menü: Tools > VR Lab > Projeyi Kur
// Klasör yapısını, Main sahnesini ve yer tutucu GameObject'leri oluşturur.
// İş bittikten sonra bu dosya silinebilir; oluşturdukları yerinde kalır.

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ProjeKurulumu
{
    const string Root = "Assets/_Project";
    const string ScenePath = Root + "/Scenes/Main.unity";

    static readonly string[] Klasorler =
    {
        "Scenes",
        "Scripts", "Scripts/Core", "Scripts/Interaction", "Scripts/Training", "Scripts/UI",
        "Materials",
        "Prefabs", "Prefabs/Environment", "Prefabs/Interaction", "Prefabs/UI",
        "Models", "Splats", "Audio", "Data"
    };

    [MenuItem("Tools/VR Lab/Projeyi Kur")]
    public static void Kur()
    {
        if (File.Exists(ScenePath) &&
            !EditorUtility.DisplayDialog("Projeyi Kur", "Main sahnesi zaten var. Üzerine yazılsın mı?", "Evet", "Hayır"))
            return;

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        // 1) Klasörler (.gitkeep sayesinde boş klasörler Git'te de görünür)
        foreach (var k in Klasorler)
        {
            string yol = Root + "/" + k;
            KlasorOlustur(yol);
            string gitkeep = yol + "/.gitkeep";
            if (!File.Exists(gitkeep)) File.WriteAllText(gitkeep, "");
        }

        // 2) Materyaller
        var mZemin  = Materyal("Floor",          new Color(0.55f, 0.55f, 0.55f));
        var mMasa   = Materyal("Table",          new Color(0.45f, 0.32f, 0.22f));
        var mCihaz  = Materyal("Device",         new Color(0.80f, 0.80f, 0.82f));
        var mPanel  = Materyal("DevicePanel",    new Color(0.10f, 0.12f, 0.15f));
        var mGuc    = Materyal("PowerButton",    new Color(0.85f, 0.15f, 0.15f));
        var mBaslat = Materyal("StartButton",    new Color(0.15f, 0.70f, 0.25f));
        var mParca  = Materyal("Part",           new Color(0.20f, 0.45f, 0.90f));
        var mYuva   = Materyal("PartSocket",     new Color(0.95f, 0.75f, 0.10f));

        // 3) Sahne (Main Camera + Directional Light ile başlar)
        var sahne = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Ortam
        var ortam = Bos("Environment", null);
        Bos("Lab_3DGS", ortam.transform);        // 3DGS modeli buraya gelecek
        Bos("TeleportAreas", ortam.transform);   // Işınlanma alanları buraya gelecek
        Sekil(PrimitiveType.Plane, "Floor", ortam.transform, Vector3.zero, new Vector3(2f, 1f, 2f), mZemin);
        Sekil(PrimitiveType.Cube, "Table", ortam.transform, new Vector3(0f, 0.375f, 2f), new Vector3(1.6f, 0.75f, 0.8f), mMasa);

        // Cihaz (masanın üstünde, 1 birim = 1 metre)
        var cihaz = Bos("Device", null);
        cihaz.transform.position = new Vector3(0f, 0.75f, 2f);
        Sekil(PrimitiveType.Cube, "DeviceBody", cihaz.transform, new Vector3(0f, 0.3f, 0f), new Vector3(0.6f, 0.6f, 0.5f), mCihaz);
        Sekil(PrimitiveType.Cube, "ParameterPanel", cihaz.transform, new Vector3(-0.1f, 0.4f, -0.255f), new Vector3(0.25f, 0.15f, 0.01f), mPanel);
        var guc = Sekil(PrimitiveType.Cylinder, "PowerButton", cihaz.transform, new Vector3(0.2f, 0.45f, -0.26f), new Vector3(0.06f, 0.01f, 0.06f), mGuc);
        guc.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        var baslat = Sekil(PrimitiveType.Cylinder, "StartButton", cihaz.transform, new Vector3(0.2f, 0.3f, -0.26f), new Vector3(0.06f, 0.01f, 0.06f), mBaslat);
        baslat.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        Sekil(PrimitiveType.Cube, "PartSocket", cihaz.transform, new Vector3(0f, 0.605f, 0f), new Vector3(0.12f, 0.01f, 0.12f), mYuva);
        Sekil(PrimitiveType.Cube, "Part", cihaz.transform, new Vector3(0.5f, 0.05f, 0f), new Vector3(0.1f, 0.1f, 0.1f), mParca);

        // Arayüz panelleri (şimdilik boş yer tutucular)
        var ui = Bos("UI", null);
        foreach (var p in new[] { "WelcomePanel", "InfoPanel", "SafetyChecklistPanel", "InstructionPanel", "ResultPanel" })
            Bos(p, ui.transform);

        // Yöneticiler
        var yoneticiler = Bos("Managers", null);
        Bos("TrainingManager", yoneticiler.transform);

        // 4) Kaydet ve Build Settings'e ekle
        EditorSceneManager.SaveScene(sahne, ScenePath);

        var liste = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!liste.Exists(s => s.path == ScenePath))
        {
            liste.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = liste.ToArray();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Ekran görüntüsü için cihazı ortala
        Selection.activeGameObject = cihaz;
        if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.FrameSelected();

        EditorUtility.DisplayDialog("Projeyi Kur", "Klasörler, Main sahnesi ve temel GameObject'ler oluşturuldu.", "Tamam");
    }

    static void KlasorOlustur(string yol)
    {
        if (AssetDatabase.IsValidFolder(yol)) return;
        string ust = Path.GetDirectoryName(yol).Replace('\\', '/');
        KlasorOlustur(ust);
        AssetDatabase.CreateFolder(ust, Path.GetFileName(yol));
    }

    static Material Materyal(string ad, Color renk)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        var m = new Material(shader);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", renk);
        else m.color = renk;
        AssetDatabase.CreateAsset(m, Root + "/Materials/" + ad + ".mat");
        return m;
    }

    static GameObject Bos(string ad, Transform ebeveyn)
    {
        var go = new GameObject(ad);
        if (ebeveyn != null) go.transform.SetParent(ebeveyn, false);
        return go;
    }

    static GameObject Sekil(PrimitiveType tur, string ad, Transform ebeveyn, Vector3 konum, Vector3 olcek, Material m)
    {
        var go = GameObject.CreatePrimitive(tur);
        go.name = ad;
        go.transform.SetParent(ebeveyn, false);
        go.transform.localPosition = konum;
        go.transform.localScale = olcek;
        go.GetComponent<Renderer>().sharedMaterial = m;
        return go;
    }
}
