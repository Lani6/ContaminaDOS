# Despliegue del API

## Paso 1: Configurar el Entorno en OCI

### 1.1 Crear una VCN (Virtual Cloud Network)

1. En la consola de OCI, navega a **Networking > Virtual Cloud Networks**.
2. Haz clic en **Create VCN** y selecciona **VCN with Internet Connectivity**.
3. Asigna un nombre a tu VCN y crea una subred pública.
4. Asegúrate de que la subred tenga una **Internet Gateway** y una **Route Table** que permita el tráfico desde y hacia Internet.
5. Añadir reglas para el puerto 80 (http) y el 443 (https)

### 1.2 Configurar Reglas de Seguridad para el Tráfico

1. Dirígete a **Networking > Network Security Groups** o a las **Security Lists** de tu VCN.
2. Añade una regla de entrada para permitir el tráfico en el puerto de tu aplicación (ej. 80 para HTTP o 443 para HTTPS).
   - **Source CIDR**: `0.0.0.0/0` (permite acceso desde cualquier IP).
   - **Destination Port**: el puerto en el que corre tu aplicación (ej. 80).
   - **Protocol**: TCP

---

## Paso 2: Crear una Instancia

### 2.1 Crear una Nueva Instancia

1. En la consola de OCI, ve a **Compute > Instances**.
2. Haz clic en **Create Instance** y selecciona el sistema operativo (ej. Ubuntu o Oracle Linux).
3. Configura los recursos según tus necesidades (tipo de máquina, CPUs y memoria).
4. Selecciona la **VCN** y la **subnet** creadas previamente y habilita una **IP pública** para que la instancia sea accesible desde Internet.
5. Revisa las configuraciones y haz clic en **Create**. Espera a que la instancia esté en estado **Running**.

---

## Paso 3 Configurar Docker en la Instancia

Comando SSH para conectarte a la instancia:

```bash
ssh -i "C:\Users\Lani0\.ssh\id_rsa" opc@149.130.180.111
```

Ejecutar:

```bash
sudo yum update -y
sudo yum install -y oracle-epel-release
sudo yum install -y nginx
sudo systemctl enable nginx
sudo systemctl start nginx
```

## Habilitar puertos en el firewall

```bash
sudo firewall-cmd --per
sudo firewall-cmd --permanent --add-service=http
sudo firewall-cmd --permanent --add-service=https
sudo firewall-cmd --reload
```

---

### 3.1 Instalar Docker en la Instancia

```bash
sudo yum install -y docker-engine
```

Inicia Docker y habilítalo para que arranque con el sistema:

```bash
sudo systemctl start docker
sudo systemctl enable docker
```

### 3.2 Verificar la Instalación de Docker

Ejecuta el siguiente comando para confirmar que Docker está instalado:

```bash
docker --version
```

---

## Paso 4: Configurar Git

### 4.1 Instalar git en la instancia

Comando para instalar git en Oracle Linux:

```bash
sudo yum install git -y
git --version
```

Se necesita tener un llave ssh en la cuenta de git y en la instancia

### 4.2 Clonar el proyecto en la instancia

Se ejecuta el siguiente comando

```bash
git clone git@github.com:Joshy17/ProyectoRedesAPI.git
```

---

## Instalación de Docker Compose

1. Descargar Docker Compose:

   ```bash
   sudo curl -L "https://github.com/docker/compose/releases/download/v2.20.2/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
   ```

2. Dar permisos de ejecución:

   ```bash
   sudo chmod +x /usr/local/bin/docker-compose
   ```

---

## Ejecutar Docker Compose para Desplegar la Aplicación

### Navegar al directorio del proyecto:

```bash
cd ProyectoRedesAPI/ProyectoRedes
```

### Ejecutar Docker Compose:

```bash
docker-compose up --build
```

Detener el contenedor

```bash
docker-compose down
```

---

## Ejecutar Docker en segundo Plano

```bash
docker-compose up --build -d
```

---

# Solución Dominio

Paula Chaves

## Interfaz OCI

1. **Crear Zona de DNS**

   - Networking
   - Zones
   - Create zone
     - Se le agrega el nombre correspondiente del grupo a la zona: grupoa.oci.meseguercr.com
   - Records
   - Add Record

     - Record type: A - Ipv4 address
     - name: www
     - Rdata mode: Basic
     - Address: Dirección IP de la instancia
     - Enviar
     - Añadir TTL de 300 segundos
     - Enviar

   - Publicar Cambios
     - Publicar Cambios

2. **Revisar si los cambios están listos**

   dig grupoa.oci.meseguercr.com

   - Se revisa que el servidor dns es el de oracle

   dig www.grupoa.oci.meseguercr.com

   - Se revisa está la IP de la instancia

3. **Crear Record para usar LetsEncrypt**
   - Add Record
     - Record type: CAA
     - name: www
     - Rdata mode: Basic
     - Flags: 0
     - Tag: issue
     - Value: letsencrypt.org
     - Enviar
   - Publish changes
     - Publish changes

## Máquina virtual

#### Paso 1: Habilitar el repositorio correcto de Certbot

1. **Agrega el repositorio de Certbot directamente desde `EPEL`**:

```bash

   sudo dnf install epel-release -y

   sudo dnf config-manager --set-enabled ol8_developer_EPEL

```

2. **Actualiza los metadatos de los repositorios:**

```bash
   sudo dnf makecache
```

#### Paso 2: Instalar Certbot

Instalar `certbot` y su plugin para Nginx:
sudo dnf install certbot python3-certbot-nginx -y

#### Paso 3: Configurar Certbot

1. Si el paso anterior fue exitoso, genera el certificado SSL:

```bash
   sudo certbot --nginx -d www.grupoa.oci.meseguercr.com
```

- Ingresar un correo valido
- Y

Esto generará los certificados en `/etc/letsencrypt/live/www.grupoa.oci.meseguercr.com/`.

---

### Paso 5: Configuración Manual de Nginx

```bash
sudo nano /etc/nginx/conf.d/default.conf
```

```bash
#Configuración para HTTPS

server {
listen 443 ssl;
server_name www.grupoa.oci.meseguercr.com;

    # Certificado SSL
    # ssl_certificate /etc/nginx/ssl/nginx.crt;
    # ssl_certificate_key /etc/nginx/ssl/nginx.key;

    # Certificado Letsencrypt
    ssl_certificate /etc/letsencrypt/live/www.grupoa.oci.meseguercr.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/www.grupoa.oci.meseguercr.com/privkey.pem;

    # Protocolos y cifrados SSL
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    # Configuración del proxy
    location / {
        proxy_pass http://localhost:8080/;  # El backend usa HTTP
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

}

# Configuración para HTTP

server {
listen 80;
server_name www.grupoa.oci.meseguercr.com;

    location / {
        proxy_pass http://localhost:8080/;  # El backend usa HTTP
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

}
```

- Usamos el siguiente comando
- sudo systemctl restart nginx

---

### 6. Acceder a tu Aplicación

Abre un navegador y accede al dominio de tu instancia:

```http
http://www.grupoa.oci.meseguercr.com/api/games
```

```https
https://www.grupoa.oci.meseguercr.com/api/games
```

---
