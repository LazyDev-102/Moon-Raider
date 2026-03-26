using UnityEngine;
using System.Collections;

public class Popper : MonoBehaviour
{
    public bool PopOnStart = true;
	public GameObject[] Item;   
	public int Count;
	public int OddsInHundred = 100;
   
    // Use this for initialization
    void Start()
    {
        if(PopOnStart)
		    Pop();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

 

    public void Pop()
    {      
        for (var n = 0; n < Count; n++)
        {
            int odds = Random.Range(0, 99);
			int i = Random.Range(0, Item.Length - 1);

			if (odds > OddsInHundred)
				continue;

            float d = (float)Random.Range(-5, 5) / 5f;


            GameObject obj = Instantiate(Item[i], transform.position + d * Vector3.one, transform.rotation);
			obj.name = "PopItem" + n;
			obj.transform.parent = gameObject.transform.parent;

			Rigidbody2D rigid = obj.GetComponent<Rigidbody2D>();

			Gem gem = obj.GetComponent<Gem>();

			if (gem)
			{
				int type = Random.Range(70, 75);
				gem.Init(type);

				gem.StartCoroutine(gem.Collect());
            
				if (rigid != null)
				{
					if (Random.Range(1, 5) < 3)
						rigid.AddForce(new Vector2(Random.Range(1, 3), 2 * Random.Range(1, 3)), ForceMode2D.Impulse);
					else
						rigid.AddForce(new Vector2(-Random.Range(1, 3), 2 * Random.Range(1, 3)), ForceMode2D.Impulse);
				}
			}
			else
			{

                Optimize opt = obj.GetComponent<Optimize>();
                if (opt == null)
                {
                    if (rigid != null)
                    {
                        if (Random.Range(1, 5) < 3)
                            rigid.AddForce(new Vector2(4.0f, Random.Range(2, 4)), ForceMode2D.Impulse);
                        else
                            rigid.AddForce(new Vector2(-4.0f, Random.Range(4, 4)), ForceMode2D.Impulse);
                    }
                }
                else
                {
                    opt.StartCoroutine(opt.SetStateDelayed(0.05f, true));
                    obj.transform.localScale = new Vector3(-1, 1, 1);
                }
			}
        }
    }
}
