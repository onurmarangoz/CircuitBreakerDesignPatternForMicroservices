# CircuitBreakerDesignPatternForMicroservices

Circuit Breaker Design Pattern için Polly kütüphanesi kullanılarak mini bir microservice örneği tasarlanmıştır.

Kütüphane içinde bulunan iki farklı circuit brekaer metodu içinde örnekler mevcuttur.
Circuit Breaker pattern; İki service birbir ile iletişim kuramadığında veya servislerden biri kapandığında,
isteği yapan servisin belirli bir zaman dilimindeki hata oranına bakarak istek yapmayı kesmesi ve belirtilen süre aralığı içinde ilgili servisin ayakta olmadığını belirten
bir hata mesajı dönmesini sağlar. İlgili süre doldugunda tekrar servis istek iletişimi açarak işlemine devam eder.
