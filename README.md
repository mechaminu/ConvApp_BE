# ���������� �鿣�� REST API + ���Ŀ ����
- ##��ǰ CRUD
	http://convappdev.azurewebsites.net/api/products
	
    - ### ��ǰ ǰ�� �� `Product.cs`
	```c#
    class Product 

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public byte Store { get; set; }
    public string Category { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Comment { get; set; }
    public string Image { get; set; }   // URI
    ```

---

- ##������ - ���� ������ CRUD
	http://convappdev.azurewebsites.net/api/posting/userrecipes

    - ###���� ������ �� `UserRecipe.cs`

    ```c#
    class UserRecipe

    [DatabaseGenerated(DatabaseGeneratedOption.Identity),Key]
    public long oid { get; set; }

    public byte[] createdate { get; set; }      // Timestamp
    public Guid createuser { get; set; }        // uniqueidentifier
    public string textcontent { get; set; }     // ntext
    public byte[] images { get; set; }          // varbinary
    ```
    - ###���������̼� ����
    |url|����|���ϰ�|
    |---|---|---|
    |`/page`|��ü ������ ��|int| 
    |`/page/{������ ��ȣ}`|�Է¹��� �������� �ش��ϴ� ����Ʈ ��� ����. �������� 0���� ����|List<UserRecipe>|

---