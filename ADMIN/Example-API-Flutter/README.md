# microservice
Microservices Architecture

# microservices implementation with ocelot gateway
https://www.c-sharpcorner.com/article/microservices-implementation-with-ocelot-gateway-using-net-core-6-api-and-angul/

# Docker
https://www.youtube.com/watch?v=l3tqRVSpBhc
https://www.youtube.com/watch?v=r6JiWwh-08c&list=PLwJr0JSP7i8At14UIC-JR4r73G4hQ1CXO&index=1

# Kubernetes
https://www.youtube.com/watch?v=TTNsHaOxU10&list=PLPA5WSROSsagXbBQTXmG9Rss-lhvAsID9&index=1
https://www.youtube.com/watch?v=yOBeQNGX278&list=PLwJr0JSP7i8D-QS50lYsXpAg-jYoqxMVy

# Entity framework
https://tuhocict.com/huong-dan-tu-hoc-lap-trinh-ado-net-va-entity-framework/
https://www.entityframeworktutorial.net/efcore/entity-framework-core-migration.aspx
add-migration <migration name>
add-migration init-db -Context DataContext
Remove-migration (xóa file migration gần nhất)
Update-database 
Update-database -Context DataContext

# JWT Authentication And Authorization With Identity Framework
https://xuanthulab.net/asp-net-razor-su-dung-identity-de-tao-user-xac-thuc-dang-nhap-website.html
https://www.c-sharpcorner.com/article/jwt-authentication-and-authorization-in-net-6-0-with-identity-framework/
https://www.c-sharpcorner.com/article/jwt-authentication-with-refresh-tokens-in-net-6-0/
https://www.c-sharpcorner.com/article/client-side-application-for-jwt-refresh-token-in-angular-13/	
https://www.c-sharpcorner.com/article/jwt-authentication-in-microservices/

# GRPC
https://learn.microsoft.com/en-us/aspnet/core/tutorials/grpc/grpc-start?view=aspnetcore-7.0&tabs=visual-studio
https://www.dotnetcoban.com/2019/08/introduction-to-grpc-on-asp-dotnet-core.html

# Deploy API
docker-compose up -d
docker-compose up -d --build (build image mới, không cần xóa image cũ)
docker-compose down

# Deploy kafka consumer
docker-compose -f docker-compose-consumer.yml up -d
docker-compose -f docker-compose-consumer.yml down

# Deploy kafka customer
docker-compose -f docker-compose-customer.yml up -d


# Port API, Kafka Consumer & Background Service phục vụ health check Container
Qaidora.Mask.UserService.API: 9093
Qaidora.Mask.Box.API: 7002 