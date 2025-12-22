import { Router } from 'express';
import { logger } from '@/config/logger';

const router = Router();

// Health check simple
router.get('/', (req, res) => {
  res.json({
    success: true,
    message: 'Essensys Backend API - Service opérationnel',
    timestamp: new Date().toISOString(),
    version: process.env.npm_package_version || '1.0.0',
    environment: process.env.NODE_ENV || 'development',
    uptime: process.uptime(),
  });
});

// Health check détaillé
router.get('/detailed', async (req, res) => {
  const healthCheck = {
    success: true,
    timestamp: new Date().toISOString(),
    service: 'essensys-backend',
    version: process.env.npm_package_version || '1.0.0',
    environment: process.env.NODE_ENV || 'development',
    uptime: process.uptime(),
    memory: process.memoryUsage(),
    checks: {
      database: 'ok', // TODO: Ajouter vérification Prisma
      redis: 'ok',     // TODO: Ajouter vérification Redis
      filesystem: 'ok',
    },
  };

  try {
    // TODO: Ajouter des vérifications réelles des services
    // - Connexion base de données
    // - Connexion Redis
    // - Espace disque
    // - Autres services critiques

    logger.info('Health check détaillé demandé', {
      ip: req.ip,
      userAgent: req.get('User-Agent'),
    });

    res.json(healthCheck);
  } catch (error) {
    logger.error('Erreur lors du health check détaillé', { error });
    
    res.status(503).json({
      ...healthCheck,
      success: false,
      error: 'Certains services ne sont pas disponibles',
    });
  }
});

// Readiness probe (pour Kubernetes)
router.get('/ready', (req, res) => {
  // TODO: Vérifier que tous les services critiques sont prêts
  res.json({
    success: true,
    message: 'Service prêt à recevoir du trafic',
    timestamp: new Date().toISOString(),
  });
});

// Liveness probe (pour Kubernetes)
router.get('/live', (req, res) => {
  res.json({
    success: true,
    message: 'Service vivant',
    timestamp: new Date().toISOString(),
  });
});

export { router as healthRoutes };