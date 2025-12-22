import winston from 'winston';

const { combine, timestamp, errors, json, colorize, simple } = winston.format;

// Configuration du logger
export const logger = winston.createLogger({
  level: process.env.LOG_LEVEL || 'info',
  format: combine(
    errors({ stack: true }),
    timestamp(),
    json()
  ),
  defaultMeta: { service: 'essensys-backend' },
  transports: [
    // Écriture dans un fichier d'erreurs
    new winston.transports.File({ 
      filename: 'logs/error.log', 
      level: 'error',
      maxsize: 5242880, // 5MB
      maxFiles: 5,
    }),
    // Écriture dans un fichier combiné
    new winston.transports.File({ 
      filename: 'logs/combined.log',
      maxsize: 5242880, // 5MB
      maxFiles: 5,
    }),
  ],
});

// En développement, ajouter aussi la console
if (process.env.NODE_ENV !== 'production') {
  logger.add(new winston.transports.Console({
    format: combine(
      colorize(),
      simple()
    )
  }));
}

// Logger spécialisé pour les requêtes HTTP
export const httpLogger = winston.createLogger({
  level: 'info',
  format: combine(
    timestamp(),
    json()
  ),
  defaultMeta: { service: 'essensys-http' },
  transports: [
    new winston.transports.File({ 
      filename: 'logs/http.log',
      maxsize: 5242880, // 5MB
      maxFiles: 5,
    }),
  ],
});

// Logger pour les actions IoT
export const iotLogger = winston.createLogger({
  level: 'info',
  format: combine(
    timestamp(),
    json()
  ),
  defaultMeta: { service: 'essensys-iot' },
  transports: [
    new winston.transports.File({ 
      filename: 'logs/iot.log',
      maxsize: 5242880, // 5MB
      maxFiles: 5,
    }),
  ],
});

export default logger;