using UnityEngine;
public class DestroyAfterTime : MonoBehaviour
{
    [SerializeField]
    private float _lifeTime;
    private TextMesh _text;
    
    void Awake()
    {
        // Set the text of the attached Text mesh
        _text = GetComponent<TextMesh>();
        Destroy(gameObject, _lifeTime);
    }

    public void SetValue(int value)
    {
        _text.text = "+" + value.ToString(); // Agregamos el s�mbolo de suma
    }


    void Update() 
    {
        transform.Translate( Time.deltaTime * Vector3.up);
    }
}
