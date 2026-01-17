# Infraestrutura D.AI Bank - Terraform Skeleton
# Este arquivo define os recursos básicos para rodar a aplicação em Cloud (ex: AWS)

provider "aws" {
  region = "us-east-1"
}

# 1. VPC & Networking
resource "aws_vpc" "fintech_vpc" {
  cidr_block = "10.0.0.0/16"
  tags = {
    Name = "DAIBank-VPC"
  }
}

# 2. Banco de Dados (DocumentDB compatível com MongoDB)
resource "aws_docdb_cluster" "mongodb" {
  cluster_identifier      = "fintech-db-cluster"
  engine                  = "docdb"
  master_username         = "fintechadmin"
  master_password         = "SecurePass123!" # Idealmente via Secret Manager
  backup_retention_period = 5
  preferred_backup_window = "07:00-09:00"
  skip_final_snapshot     = true
}

# 3. Cache & Idempotência (ElastiCache Redis)
resource "aws_elasticache_cluster" "redis" {
  cluster_id           = "fintech-redis"
  engine               = "redis"
  node_type            = "cache.t3.micro"
  num_cache_nodes      = 1
  parameter_group_name = "default.redis7"
  port                 = 6379
}

# 4. Mensageria (Amazon MQ ou RabbitMQ Auto-gerenciado)
resource "aws_mq_broker" "rabbitmq" {
  broker_name = "fintech-messaging"

  engine_type        = "RabbitMQ"
  engine_version     = "3.10.20"
  host_instance_type = "mq.t3.micro"

  user {
    username = "fintechuser"
    password = "SecurePass123!"
  }
}

# 5. Cluster de Aplicação (ECS/Fargate)
resource "aws_ecs_cluster" "fintech_cluster" {
  name = "DAIBank-Cluster"
}

# Output das URLs de Conexão
output "mongodb_endpoint" {
  value = aws_docdb_cluster.mongodb.endpoint
}

output "redis_endpoint" {
  value = aws_elasticache_cluster.redis.cache_nodes[0].address
}
