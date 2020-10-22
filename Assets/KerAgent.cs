using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;

public class KerAgent : Agent
{
    public GameObject cat1;
    public GameObject cat2;
    public GameObject dog2;
    public GameObject[] obstacles;
    public Transform field;
    public float rotationMultiplier = 3f;
    public float translationMultiplier = 5f;

    public AudioClip winAudio;
    public AudioClip loseAudio;

    public Text pointsText;

    private CharacterController characterController;
    private AudioSource audioSource;

    private int playingFieldWidth = 15;
    private int points = 0;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void Initialize()
    {

    }

    public override void CollectObservations(Unity.MLAgents.Sensors.VectorSensor sensor)
    {
        sensor.AddObservation(transform.position);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        AddReward(-1.0f / MaxStep);

        var actionRotateY = Mathf.Clamp(vectorAction[0], -rotationMultiplier, rotationMultiplier);
        var actionForward = Mathf.Clamp(vectorAction[1], -translationMultiplier, translationMultiplier);

        transform.Rotate(Vector3.up, actionRotateY);
        characterController.SimpleMove(transform.forward * actionForward);

        var xDiff = field.position.x - transform.position.x;
        var yDiff = field.position.z - transform.position.z;

        if (Mathf.Abs(xDiff) > playingFieldWidth || (Mathf.Abs(yDiff) > playingFieldWidth))
        {
            AddReward(-1);
            points--;
            audioSource.PlayOneShot(loseAudio);
            EndEpisode();
        }
    }

    public void OnTriggerEnter(Collider collider)
    {
        Debug.Log(collider.tag);

        if (collider.tag == "cat")
        {
            AddReward(1);
            points ++;
            audioSource.PlayOneShot(winAudio);
            EndEpisode();
        }
        if (collider.tag == "dog")
        {
            AddReward(-1);
            points--;
            audioSource.PlayOneShot(loseAudio);
            EndEpisode();
        }
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Horizontal") * rotationMultiplier;
        actionsOut[1] = Input.GetAxis("Vertical") * translationMultiplier;
    }

    public override void OnEpisodeBegin()
    {
        // Put dogs on a random position within the field
        var posOffsetDog = new Vector3(Random.Range(-playingFieldWidth,playingFieldWidth), 0, Random.Range(-playingFieldWidth,playingFieldWidth));
        transform.position = field.position + posOffsetDog;
        posOffsetDog = new Vector3(Random.Range(-playingFieldWidth,playingFieldWidth), 0, Random.Range(-playingFieldWidth,playingFieldWidth));
        dog2.transform.position = field.position + posOffsetDog;

        // Put cats on a random positions within the field
        var posOffsetCat = new Vector3(Random.Range(-playingFieldWidth,playingFieldWidth), 0, Random.Range(-playingFieldWidth,playingFieldWidth));
        cat1.transform.position = field.position + posOffsetCat;   
        posOffsetCat = new Vector3(Random.Range(-playingFieldWidth,playingFieldWidth), 0, Random.Range(-playingFieldWidth,playingFieldWidth));
        cat2.transform.position = field.position + posOffsetCat;

        // Choose a random obstacle set
        int randomObstacle = Random.Range(0, obstacles.Length);
        for (int i = 0; i < obstacles.Length; i++)
        {
            obstacles[i].SetActive(i == randomObstacle);
        }

        pointsText.text = points.ToString();
    }
}
