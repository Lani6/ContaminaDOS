const express = require('express');
const fs = require('fs');
const https = require('https');
const path = require('path');
const next = require('next');

// Define si estás en modo desarrollo
const dev = process.env.NODE_ENV !== 'production';
const appNext = next({ dev });
const handle = appNext.getRequestHandler();

// Inicializar la aplicación Express
const app = express();

// Leer los certificados para HTTPS
const httpsOptions = {
  key: fs.readFileSync(path.join(__dirname, 'certs', 'localhost-key.pem')),
  cert: fs.readFileSync(path.join(__dirname, 'certs', 'localhost.pem')),
};

// Preparar Next.js
appNext.prepare().then(() => {
  // Redirigir todo el tráfico a Next.js (esto incluye todas las rutas)
  app.all('*', (req, res) => {
    return handle(req, res);
  });

  // Levantar servidor HTTPS en el puerto 3443
  https.createServer(httpsOptions, app).listen(3443, (err) => {
    if (err) {
      console.error('Error al iniciar el servidor HTTPS:', err);
      return;
    }
    console.log('Servidor HTTPS corriendo en https://localhost:3443');
  });

  // Levantar servidor HTTP en el puerto 3002
  app.listen(3002, (err) => {
    if (err) {
      console.error('Error al iniciar el servidor HTTP:', err);
      return;
    }
    console.log('Servidor HTTP corriendo en http://localhost:3002');
  });
}).catch(err => {
  console.error('Error durante la preparación de Next.js:', err);
});