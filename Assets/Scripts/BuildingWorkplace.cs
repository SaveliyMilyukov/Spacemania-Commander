using UnityEngine;
using UnityEngine.UI;

public class BuildingWorkplace : Unit
{
    public GameObject buildingPrefab;
    [Space(5)]
    [SerializeField] float currentBuildTime; // Сколько времени уже строится
    [SerializeField] float buildTime; // Объем работы по времени на строительство
    [SerializeField] Sprite stage1, stage2, stage3; // Спрайты разных стадий строительства
    [SerializeField] SpriteRenderer render;
    [Space(5)]
    [SerializeField] bool autoBuild = false;
    [SerializeField]Image progressBar;

    public override void Awake()
    {
        base.Awake();
    }

    
    public override void Update()
    {
        base.Update();

        if (autoBuild) AddProgress(1f);
        progressBar.fillAmount = currentBuildTime / buildTime;
    }

    public void SetBuildTime(float time_)
    {
        buildTime = time_;
        currentBuildTime = 0;
    }

    public virtual void AddProgress(float speed_)
    {
        // Прибавляем к текущему времени строительства переданное на входе в функцию значение умноженное на время (deltaT)
        currentBuildTime += speed_ * Time.deltaTime;

        // Подбор спрайта под стадию строительства
        if(currentBuildTime > buildTime / 3 * 2) // Если уже 2/3 построено
        {
            render.sprite = stage3;
        }
        else if (currentBuildTime > buildTime / 3) // Если уже 1/3 построена
        {
            render.sprite = stage2;
        }
        else // Если работа еще только началась (построено < 1/3)
        {
            render.sprite = stage1;
        }

        // Если текущее время строительство больше или равно времени на строительство здания
        if (currentBuildTime >= buildTime)
        {
            FinishWork(); // Завершаем работу
        }
    }

    public virtual void FinishWork()
    {
        Debug.Log("Job's finished! (" + gameObject.name + "/" + buildingPrefab.name + ")");

        Building b = Instantiate(buildingPrefab, transform.position, Quaternion.identity).GetComponent<Building>();
        b.playerNumber = playerNumber;

        if(PlayerController.localPlayer.unitsAndConstructions.Contains(this)) PlayerController.localPlayer.unitsAndConstructions.Remove(this);
        PlayerController.localPlayer.UpdateUnits();

        Destroy(gameObject);
    }
}
