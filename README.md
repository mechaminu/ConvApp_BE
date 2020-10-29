# 편의점달인 백엔드 REST API + 백워커 서버
- ##상품 CRUD
	http://convappdev.azurewebsites.net/api/products
	
    - ### 상품 품목 모델 `Product.cs`
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

- ##포스팅 - 유저 레시피 CRUD
	http://convappdev.azurewebsites.net/api/posting/userrecipes

    - ###유저 레시피 모델 `UserRecipe.cs`

    ```c#
    class UserRecipe

    [DatabaseGenerated(DatabaseGeneratedOption.Identity),Key]
    public long oid { get; set; }

    public byte[] createdate { get; set; }      // Timestamp
    public Guid createuser { get; set; }        // uniqueidentifier
    public string textcontent { get; set; }     // ntext
    public byte[] images { get; set; }          // varbinary
    ```
    - ###페이지네이션 지원
    |url|설명|리턴값|
    |---|---|---|
    |`/page`|전체 페이지 수|int| 
    |`/page/{페이지 번호}`|입력받은 페이지에 해당하는 포스트 목록 리턴. 페이지는 0부터 시작|List<UserRecipe>|

---