# Usa una imagen de node como base 
FROM node:latest 

RUN apt-get update && apt-get install -y dos2unix
# Establece el directorio de trabajo dentro del contenedor 
WORKDIR /app 

# Copia los archivos del proyecto al contenedor 
COPY package*.json ./
# Copia los archivos del proyecto al contenedor 
COPY . . 

WORKDIR /app/my-nextjs-app 

# Instala las dependencias del proyecto 
RUN npm install 

COPY . ./

RUN chmod +x write_env.sh

EXPOSE 3000 

RUN dos2unix write_env.sh && chmod +x write_env.sh
CMD ./write_env.sh && node server
