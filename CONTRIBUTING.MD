# Contribution Guidelines

## C-Sharp & Unity

### General Guidelines for working with Unity Scripts

- All Scripts must be placed in \<UnityProject\>\Scripts or a subfolder 
- Scripts should only contain 1 class
- The Filename of the Script must match the class-name  
>For Example:  
>```csharp
>// This file must be called PlayerController.cs because 
>// the class is also called Player Controller
>class PlayerController  : MonoBehaviour 
>{
>   void Start() {}
>}
>```

### Guidelines for working with C#

- Class-Names should be UpperCamelCase
- Public member functions should be UpperCamelCase
- Public member variables should be UpperCamelCase
- Private member functions should be lowerCamelCase
- Private member variables should be m_ lowerCamelCase
>For Example:  
>```csharp
>class PlayerController  : MonoBehaviour 
>{
>    // private member variable    
>    private Vector2 m_position;
>    
>    // public member variable (or getter)
>    public Vector2 Position => m_position;
>    
>    // public method
>    public void UpdatePosition(Vector2 diff)
>    {
>        setPosition(m_position+diff);
>    }
> 
>    // private method
>    private void setPosition(Vector2 position)
>    {
>       if (position.x < 0 || position.y < 0)
>       {
>           throw new BadPositionError("Position was negative");
>       }
>       else
>       {
>           m_position = position;
>       }
>    }
>}
>```

- Prefer foreach loops over for loops when possible
>For Example:
>```csharp
>public void IterateCollection()
>{
>    List<int> collection = GetSomeFakeData();
>    int sum;
>
>    //bad
>    for(int i = 0; i < collection.Length; i++)
>    {
>       sum += collection[i];
>    }
>
>    //good
>    foreach(int item in collection)
>    {
>       sum += item;
>    }
>}

- Use Plenty of Comments
  - Explain the Usage of a method, document side-effects
  - Explain the inner workings of your code

>For Example:
>```csharp
>
>// Sets the Players position, if the position is x < 0 or y < 0 this throws a
>// BadPositionException
>public void SetPosition(Vector3 position)
>{
>    // check if the position is valid.
>    if(position.x < 0 || positiony.< 0)
>        throw new BadPositionException("position.x or position.y was negative");
>
>    m_position = position;
>}
>```


### Guidelines for working with Unity-Scripts

- prefer `[SerializeField] private` over `public` members
- use `[RequireComponent(typeof())]` over null checks
- use `[Header()]` and `[Tooltip]` to improve the editor experience.
>For Example:
>```csharp
>    [RequireComponent(typeof(MovementController))]
>    public class PlayerController : MonoBehaviour 
>    {
>        
>        private MovementController m_movement = null;
>
>        [Header(" --- Setup ---")]
>        [Tooltip("Adjusts the Jump-Height")]
>        [SerializeField] private float m_jumpHeight = 2; 
>
>        [Tooltip("The key with which the player jumps")]
>        [SerializeField] private KeyCode m_jumpKey = KeyCode.Space; // <-- Set sensible values as defaults
>
>
>        public void Start()
>        {
>            // The component was required, so we can ommit the null check
>            m_movement = gameObject.GetComponent<MovementController>();
>        }
>
>        public void Update()
>        {
>            if(Input.GetKeyDown(m_jumpKey))
>            {
>                m_movement.Jump(m_jumpHeight);
>            }
>        }
>    }
>```

- Do not iterate the Scene-Graph, use Tags
>For Example:
>```csharp
>public GameObject FindPlayer()
>{
>    return GameObject.FindWithTag("player");
>}
>```

- Use CompareTag instead of ==
>For Example:
>```csharp
>    public void OnTriggerEnter(Collider other)
>    {
>        // bad
>        if(other.gameObject.tag == "Player")
>        {
>            HandlePlayerEnter(other)
>        }
>    }
>
>    public void OnTriggerEnter(Collider other)
>    {
>        // good
>        if(other.CompareTag("Player"))
>        {
>            HandlePlayerEnter(other)
>        }
>    }
>```

- Make your Code Event-Driven
>For Example:
>```csharp
>    public class CollisionManager {
>
>        // bad
>        public void HandlePlayerEnter(Collider other)
>        {
>            ResolveCollision(other,m_player);
>            PlayCollisionSound();
>            DamagePlayer();
>            etc...
>        }
>    }
>```
>
>```csharp
>    public class CollisionManager {
>
>        public event Action<> OnPlayerObstacleCollision = null;
>
>        public void HandlePlayerEnter(Collider other)
>        {
>            ResolveCollision(other,m_player);
>
>            OnPlayerObstacleCollision?.Invoke();
>        }
>
>    }
>```
>
>You can then create secondary classes like this
>```csharp
>    // External Class for playing sounds
>    public class SoundPlayer : MonoBehaviour {
>        public void Start()
>        {
>            collisionManager = GameObject
>            .FindWithTag("CollisionManager")
>            ??.GetComponent<CollisionManager>();
>
>            collisionManager.OnPlayerObstacleCollision += PlaySound();
>        }
>
>        public void PlaySound() {
>            ...
>        }
>
>    }
>```
> Take a look at `EventBus.cs` for some more advanced examples


## Assets

Assets should use a unified file-structure

### Images
- Images should live in `Assets/Images` or in a subfolder
- They should follow the general-guideline of:  
`descriptive_file_name-<widht>x<height>-rev<revision>.extension`  
    > For example `player_sprite-64x64-rev1.png`

    If only a single size of the image exists the size specified can be dropped:  
    > For instance `pickaxe_tool-rev0.png`

    Use underscores instead of capital letters! It avoids issues with the VCS

### Models
- Models should live in `Assets/Models` or in a subfolder
- They should follow the general-guideline of:  
`descriptive_file_name-rev<revision>.extension`
    > For example `hand_model-rev5.gltf`


### Audio
- Audio should live in `Assets/Audio` or in a subfolder
- They should follow the general-guideline of:  
`descriptive_file_name-<channels>-<bits>b-<sample-rate>kHz-rev<revision>.extension`
    > For instance `player_hurt-mono-16b-44.1kHz-rev0.mp3`

    if no alternative formats exists `bits` and `sample-rate` can be dropped
    > For example `enemy_hurt-mono-rev2.wav`




## Python
[PEP-8](https://www.python.org/dev/peps/pep-0008/), use an IDE like PyCharm. It applies these rules automatically


## Working with GIT
- pull from develop
- create a new feature-branch with your favourite tool  
or like this  `git checkout -b feature/my_cool_feature`
- add your changes, make sure that commits do not get too large
- push your changes to the remote feature branch
- create a pull request, if applicable mention the Jira-Issue   
For instance `[IA-2] [ADD] Contribution Guidelines`
- Make sure your PR is descriptive, link issues if applicable, add pictures, etc...
- wait for a review & CI
- Apply the requested changes if any
- merge & delete the branch

### Appendix: Git with Unity
- When working on the Unity-Scene make sure to make a copy first.  
 It is much easier to create prefabs then merging diverged Unity-Scenes