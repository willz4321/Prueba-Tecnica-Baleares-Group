Aquí tienes la corrección y mejora del texto:

---

**SE PROCEDERÁ A INDICAR EL FUNCIONAMIENTO DE LA APLICACIÓN:**

**INTERFAZ DE USUARIO:**

El frontend se desarrolló con vistas de blazor usando el patro MVC, para poder interactuar de manera mas amigable y dinámica con el servidor. todas las vistas se encuentran en la carpeta Views,
cuando la aplicacion se inicia se abre automáticamente la interfaz gráfica en la pagina de home en el caso de que el token de autenticación este vigente, de lo contrario sera redirigido a login.
 Los controllers de dichas vistas se encuentran en AuthViewController  y HomeController (para login, register, edit y delete de usuarios y contactos), cuenta con protección de rutas verificando  para la interfaz de usuario, dependiendo si esta  logueado, si es usuario ADMIN o no.


**AUTENTICACION:**
   
Para la interfaz grafica se utilizan cookies para almacenar datos de usuario, ademas del token con una duracion de 20 minutos y para la API Rest se utiliza JWT para dicha configuración tambien, esta logica se encuentra en el archivo program.cs, configurando la autenticación en ambos caso.

**PERMISOS:**
- Los usuarios ADMIN tendrán permisos para CRUD de usuarios y contactos.
- Los usuarios REGULAR solo podrán ver contactos, además de editar su perfil (avatar, edad y correo). No tendrán acceso a la sección de usuarios.


**BACKEND:**

El backend se desarrolló con C# .NET 8, Se utilizó Visual Studio (no VS Code) por las facilidades que ofrece. Se empleó Entity Framework con SQL Server para la gestión de la base de datos, JWT para la autenticación y autorización, además de encriptación de contraseñas.

La Arquitectura usada es una Arquitectura de capas, lo que facilita el mantenimiento y escalabilidad del sistema, reutilidad de clases services y la facilidad de agregar nuevas clases sin afectar el codigo.

**PERMISOS:**

Las validaciones de seguridad están implementadas tanto del lado del cliente como del servidor. Por lo tanto, si un usuario no tiene los permisos correspondientes, no podrá realizar operaciones CRUD.

**EJECUCIÓN:**

Si se desea probar la API Rest se puede hacerlo a traves de Swagger a traves de la url http://localhost:5260/swagger/index.html donde los endpoint que conrresponden a la API Rest son los que corresponden a Auth y Contact, AuthView y home pertenecen  a la interfaz grafica.

Login: para poder probar todos los metodos primero se debe hacer un login de usuario en el endpoint (/Auth/login) en caso de ser exitoso el login, se te devolvera el usuario con el token de autenticacion para probar
los demas metodos, por lo que al token debes colocarlo en el headers para utilizar los demas endpoint protegidos, dependiendo los permisos podras realizar una accion u otra.

  Usuario ADMIN: 
   usuario: admin@admin.com
   clave: admin

 usuario REGULAR:
  usuario: regular@regular.com
  clave: Regular 

  **ENDPOINT:**
  
  Se crearon multiples endpoint para usuarios y contactos:
 
                /* USUARIOS */
1. **Obtener Usuarios (GET /users)**
   - Este endpoint obtiene una lista de todos los usuarios registrados, pero solo es accesible para administradores (`ADMIN`).
   
2. **Registrar Usuario (POST /registerAPI)**
   - Permite registrar un nuevo usuario. Si es el primer usuario registrado, se le asigna el rol de `ADMIN`, de lo contrario se asigna el rol `REGULAR`. 
   - Verifica si el correo electrónico ya está registrado. Si es así, responde con un error.

3. **Crear Usuario (POST /createAPI)**
   - Solo accesible para administradores (`ADMIN`).
   - Permite crear un nuevo usuario con un correo no registrado. Se asigna el rol `REGULAR` y se encripta la contraseña.

4. **Editar Usuario (PUT /edituserAPI/{id})**
   - Solo accesible para administradores (`ADMIN`).
   - Permite actualizar los datos de un usuario específico, incluyendo su nombre, correo y avatar. Si la contraseña es proporcionada, se encripta.
   - Verifica que el correo no esté en uso por otro usuario.

5. **Editar Mi Usuario (PUT /editmyuserAPI/{id})**
   - Permite que un usuario regular edite su propia información (nombre, correo y avatar).
   - El correo proporcionado se verifica para asegurarse de que no esté registrado a otro usuario.

6. **Eliminar Usuario (DELETE /deleteuserAPI/{id})**
   - Solo accesible para administradores (`ADMIN`).
   - Elimina un usuario específico identificado por su ID. Si el usuario no existe, responde con un error.

7. **Iniciar Sesión (POST /login)**
   - Permite a un usuario iniciar sesión proporcionando su correo y contraseña.
   - Si las credenciales son correctas, se genera un token JWT que puede ser usado para autenticar futuras solicitudes.

8. **Renovar Token (GET /renew)**
   - Permite renovar el token JWT. El usuario debe enviar el token actual en los encabezados.
   - Si el token es válido, se genera un nuevo token JWT.

9. **Resetear Contraseña (GET /reset-password)**
   - Permite enviar un correo electrónico con un enlace para restablecer la contraseña, utilizando un token JWT para verificar la solicitud.
   - Si el correo no está registrado, se responde con un error.

10. **Confirmar Restablecimiento de Contraseña (POST /reset-password-confirm)**
    - Permite confirmar el restablecimiento de la contraseña. El usuario debe proporcionar el token JWT recibido en el correo de restablecimiento y la nueva contraseña.
    - Si el token es inválido o ha expirado, responde con un error.

**Flujo general:**
- Los endpoints que modifican la base de datos, como registrar o editar usuarios, están protegidos por roles de acceso (`ADMIN`), mientras que los usuarios regulares pueden editar su propia información y acceder a funcionalidades de inicio de sesión y restablecimiento de contraseña.
- Los tokens JWT se utilizan para autenticar y autorizar el acceso a recursos protegidos, garantizando la seguridad de las acciones realizadas por los usuarios.

                         /*CONTACTOS*/

1. **`GET api/contact`** (`GetAllContactsAPI`):
   - Obtiene todos los contactos.
   - Responde con `200 OK` y la lista de contactos o un error si ocurre un problema.

2. **`GET api/contact/{id}`** (`GetContactByIdAPI`):
   - Obtiene un contacto por su ID.
   - Si el contacto no se encuentra, devuelve un `404 Not Found` con un mensaje.
   - Si se encuentra, responde con el contacto en formato `200 OK`.

3. **`POST api/contact`** (`SaveContactAPI`):
   - Guarda un nuevo contacto, solo accesible para usuarios con el rol "ADMIN".
   - Si el contacto se guarda correctamente, responde con un `201 Created`, incluyendo el ID del nuevo contacto.
   - Si no puede guardarse, devuelve un `400 Bad Request` con un mensaje de error.

4. **`PUT api/contact`** (`UpdateContactAPI`):
   - Actualiza un contacto existente, solo accesible para usuarios con el rol "ADMIN".
   - Si el contacto se actualiza correctamente, responde con un `200 OK`.
   - Si no se encuentra el contacto, responde con un `404 Not Found`.

5. **`DELETE api/contact/{id}`** (`DeleteContactAPI`):
   - Elimina un contacto por su ID, solo accesible para usuarios con el rol "ADMIN".
   - Si el contacto se elimina correctamente, responde con `204 No Content`.
   - Si no se encuentra el contacto, responde con un `404 Not Found`.

6. **`GET api/contact/by-email/{email}`** (`GetContactByEmailAPI`):
   - Obtiene un contacto por su email.
   - Si no se encuentra, devuelve un `404 Not Found` con un mensaje adecuado.
   - Si se encuentra, responde con el contacto en formato `200 OK`.

7. **`GET api/contact/by-city/{city}`** (`GetContactsByCityAPI`):
   - Obtiene contactos por ciudad.
   - Si no hay contactos en la ciudad especificada, responde con `404 Not Found`.
   - Si hay contactos, responde con `200 OK` y la lista de contactos.

8. **`GET api/contact/getAll-by-email`** (`GetContactsSortedByEmailAPI`):
   - Obtiene todos los contactos y los ordena por su email.
   - Responde con los contactos ordenados en formato `200 OK`.

### Funciones auxiliares:
- **`HandleException`**: Una función privada para manejar excepciones y devolver un mensaje de error genérico con un código de estado `500 Internal Server Error` en caso de fallos.

### Consideraciones:
- **Seguridad**: La mayoría de las rutas que modifican o eliminan datos (POST, PUT, DELETE) requieren autenticación y roles específicos (como "ADMIN").
- **Excepciones**: Cada acción en el controlador tiene un bloque `try-catch` para capturar y manejar excepciones, garantizando que el servidor responda con un mensaje adecuado si algo sale mal.


    **ACTIDAD MAS DESAFIANTE:**

    lo mas desafiente fue configurar los endpoint de la api y el MVC de manera correcta para que no haya conflictos entre si, principalemente configurando la autenticacion con cookies y JWT se hicieron varias pruebas para por separado y en conjunto para dejar en funcionamiento la API. Ya que un problema que ocurria es que al hacer una peticion a la API rest esta te devolvia codigo html correspondiente a los controllers de la interfaz grafica, para esto se coloco  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] en los metodos correspondientes a la API rest, Este parámetro especifica el esquema de autenticación que se debe usar es  basada en JWT  utilizando el esquema JwtBearerDefaults.AuthenticationScheme.


       **BASE DE DATOS**    


 Se genero una copia de la base de datos llamada BalearesGroup que es el nombre oficial para la conexion a la misma. En el archivo appsettings.json se configuro la conexion a localDb por lo que si no tienes disponible dicha conexion deberas configurar el server con tus crendenciales y luego importar la base de datos.      


---

