import express from 'express';
import cors from 'cors';
import helmet from 'helmet';
import compression from 'compression';
import morgan from 'morgan';
import rateLimit from 'express-rate-limit';
import dotenv from 'dotenv';

import { logger } from '@/config/logger';
import { errorHandler } from '@/middleware/errorHandler';
import { notFoundHandler } from '@/middleware/notFoundHandler';
import { authRoutes } from '@/routes/authRoutes';
import { userRoutes } from '@/routes/userRoutes';
import { machineRoutes } from '@/routes/machineRoutes';
import { deviceRoutes } from '@/routes/deviceRoutes';
import { actionRoutes } from '@/routes/actionRoutes';
import { healthRoutes } from '@/routes/healthRoutes';

// Charger les variables d'environnement
dotenv.config();

const app = express();
const PORT = process.env.PORT || 3000;

// Middleware de sécurité
app.use(helmet({
  contentSecurityPolicy: {
    directives: {
      defaultSrc: ["'self'"],
      styleSrc: ["'self'", "'unsafe-inline'"],
      scriptSrc: ["'self'"],
      imgSrc: ["'self'", "data:", "https:"],
    },
  },
}));

// CORS configuration
app.use(cors({
  origin: process.env.FRONTEND_URL || 'http://localhost:3000',
  credentials: true,
  methods: ['GET', 'POST', 'PUT', 'DELETE', 'PATCH', 'OPTIONS'],
  allowedHeaders: ['Content-Type', 'Authorization', 'X-Machine-Token'],
}));

// Compression
app.use(compression());

// Rate limiting
const limiter = rateLimit({
  windowMs: 15 * 60 * 1000, // 15 minutes
  max: 100, // limite chaque IP à 100 requêtes par windowMs
  message: 'Trop de requêtes depuis cette IP, réessayez plus tard.',
  standardHeaders: true,
  legacyHeaders: false,
});
app.use(limiter);

// Body parsing
app.use(express.json({ limit: '10mb' }));
app.use(express.urlencoded({ extended: true, limit: '10mb' }));

// Logging
app.use(morgan('combined', {
  stream: {
    write: (message: string) => logger.info(message.trim())
  }
}));

// Routes
app.use('/api/health', healthRoutes);
app.use('/api/auth', authRoutes);
app.use('/api/users', userRoutes);
app.use('/api/machines', machineRoutes);
app.use('/api/devices', deviceRoutes);
app.use('/api/actions', actionRoutes);

// Middleware de gestion d'erreurs
app.use(notFoundHandler);
app.use(errorHandler);

// Démarrage du serveur
const server = app.listen(PORT, () => {
  logger.info(`🚀 Serveur Essensys démarré sur le port ${PORT}`);
  logger.info(`📚 Documentation API: http://localhost:${PORT}/api/docs`);
  logger.info(`🏥 Health check: http://localhost:${PORT}/api/health`);
});

// Gestion gracieuse de l'arrêt
process.on('SIGTERM', () => {
  logger.info('SIGTERM reçu, arrêt gracieux du serveur...');
  server.close(() => {
    logger.info('Serveur arrêté');
    process.exit(0);
  });
});

process.on('SIGINT', () => {
  logger.info('SIGINT reçu, arrêt gracieux du serveur...');
  server.close(() => {
    logger.info('Serveur arrêté');
    process.exit(0);
  });
});

export default app;