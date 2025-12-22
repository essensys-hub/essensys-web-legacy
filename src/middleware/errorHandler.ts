import { Request, Response, NextFunction } from 'express';
import { logger } from '@/config/logger';
import { AppError } from '@/types';

export const errorHandler = (
  err: Error | AppError,
  req: Request,
  res: Response,
  next: NextFunction
) => {
  // Log de l'erreur
  logger.error('Erreur capturée par le middleware:', {
    error: err.message,
    stack: err.stack,
    url: req.url,
    method: req.method,
    ip: req.ip,
    userAgent: req.get('User-Agent'),
  });

  // Erreur opérationnelle connue
  if (err instanceof AppError || (err as AppError).isOperational) {
    const appError = err as AppError;
    return res.status(appError.statusCode).json({
      success: false,
      error: appError.message,
      ...(process.env.NODE_ENV === 'development' && { stack: appError.stack }),
    });
  }

  // Erreur de validation Zod
  if (err.name === 'ZodError') {
    return res.status(400).json({
      success: false,
      error: 'Données de requête invalides',
      details: (err as any).errors,
    });
  }

  // Erreur JWT
  if (err.name === 'JsonWebTokenError') {
    return res.status(401).json({
      success: false,
      error: 'Token invalide',
    });
  }

  if (err.name === 'TokenExpiredError') {
    return res.status(401).json({
      success: false,
      error: 'Token expiré',
    });
  }

  // Erreur Prisma
  if (err.name === 'PrismaClientKnownRequestError') {
    const prismaError = err as any;
    
    // Violation de contrainte unique
    if (prismaError.code === 'P2002') {
      return res.status(409).json({
        success: false,
        error: 'Cette ressource existe déjà',
        field: prismaError.meta?.target?.[0],
      });
    }
    
    // Enregistrement non trouvé
    if (prismaError.code === 'P2025') {
      return res.status(404).json({
        success: false,
        error: 'Ressource non trouvée',
      });
    }
  }

  // Erreur de syntaxe JSON
  if (err instanceof SyntaxError && 'body' in err) {
    return res.status(400).json({
      success: false,
      error: 'JSON invalide dans le corps de la requête',
    });
  }

  // Erreur inconnue - ne pas exposer les détails en production
  const statusCode = 500;
  const message = process.env.NODE_ENV === 'production' 
    ? 'Erreur interne du serveur' 
    : err.message;

  res.status(statusCode).json({
    success: false,
    error: message,
    ...(process.env.NODE_ENV === 'development' && { stack: err.stack }),
  });
};

// Classe pour les erreurs applicatives
export class AppError extends Error implements AppError {
  public readonly statusCode: number;
  public readonly isOperational: boolean;

  constructor(message: string, statusCode: number = 500, isOperational: boolean = true) {
    super(message);
    this.statusCode = statusCode;
    this.isOperational = isOperational;

    Error.captureStackTrace(this, this.constructor);
  }
}

// Erreurs prédéfinies courantes
export class NotFoundError extends AppError {
  constructor(resource: string = 'Ressource') {
    super(`${resource} non trouvée`, 404);
  }
}

export class UnauthorizedError extends AppError {
  constructor(message: string = 'Non autorisé') {
    super(message, 401);
  }
}

export class ForbiddenError extends AppError {
  constructor(message: string = 'Accès interdit') {
    super(message, 403);
  }
}

export class ValidationError extends AppError {
  constructor(message: string = 'Données invalides') {
    super(message, 400);
  }
}

export class ConflictError extends AppError {
  constructor(message: string = 'Conflit de ressource') {
    super(message, 409);
  }
}