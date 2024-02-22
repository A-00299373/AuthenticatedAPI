# AuthenticatedAPI
Hii professor when i tried to add product server was responding with 201 with the below code.
{
  "id": 1,
  "price": 1234567890,
  "name": "asdfghjklqwertyuioop",
  "description": "kjhgfhjkhgfhjkhgfdghjkljhgfhjkl",
  "category": {
    "id": 1,
    "description1": "kjhgfdtccfghjkl"
  }
}

but when i try to find the product by the Id it was showing me below output even i entered data under "Category".
[
  {
    "id": 1,
    "price": 1234567890,
    "name": "asdfghjklqwertyuioop",
    "description": "kjhgfhjkhgfhjkhgfdghjkljhgfhjkl",
    "category": null
  }
]

I have tried by removing question mark in this field( public Category Category { get; set; } ) but still i am not able to understand why it is throwing that category is null.