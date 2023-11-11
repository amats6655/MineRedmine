# MineRedmine

## Что такое MineRedmine
MineRedmine - это проект, разработанный для адаптации платформы Redmine к мобильным устройствам.

## Чем отличается от стандартного Redmine
Хотя мобильный интерфейс Redmine частично оптимизирован под работу с мобильными устройствами, однако в нем есть ряд существенных недостатков, которые мешают комфортно работе с мобильных устройств 
1. Просмотр списка всех задач требует от пользователя использовать горизонтальный и вертикальный скрол для того чтобы увидеть общую информацию о заявках
2. Для получения списка своих задач необходимо совершить большое количество шагов -
   * Авторизоваться
   * Открыть боковое меню
   * Перейти во вкладку "Мои задания"
   * Открыть боковое меню
   * Перейти в заранее настроенный фильтр


## Основные функции

### Просмотр списка заявок
После успешной авторизации, отображается список ваших заявок. Здесь можно увидеть основную информацию о каждой заявке. Также цветом выделены сами карточки заявок на основе их приоритета - 
- ![низкий](https://placehold.it/15/2ad667/000000?text=+) Низкий,
- ![средний](https://placehold.it/15/3a63d9/000000?text=+) Средний,
- ![высокий](https://placehold.it/15/fff333/000000?text=+) Высокий,
-  ![критический](https://placehold.it/15/b03a3e/000000?text=+) Критический.

 В данном списке отображатся только заявки, соответствующие следующим условиям:
 1. Статус заявки - **В ожидании**, **В работе**, **Назначена**
 2. Заявка назначена на авторизованного пользователя, либо на группы в которых он является исполнителем

### Просмотр детальной информациии о заявке
Перейдя на любую из доступных заявок, будет отображена детальная информация о ней, включающая описание заявки, номер телефона для связи и комментарии. С этой страницы также можно взять заявку в работу.

### Фильтрация заявок
Для более удобного управления заявками, вы можете использовать фильтры по приоритету, местоположению и статусу заявки. Фильтры можно объединять.
Список местоположений для фильтра формируется динамически, на основе того, какие объекты есть в доступных заявках

Данные о фильтрации кэшируются на время сессии, и для того чтобы снова видеть все заявки, нужно нажать кнопку "All" во вкладке фильтров

### Поиск заявки по ID
Для поиска заявки, нажмите на кнопку ≡ в верхнем правом углу, введите номер заявки и нажмите "Найти"

<img src="https://github.com/amats6655/MineRedmine/assets/29190747/ef8cf905-f452-4d50-9bf8-6da5c98621ee" width=40%>
<img src="https://github.com/amats6655/MineRedmine/assets/29190747/0bb669da-397a-4889-8000-662014a763e1" width=40%>
<img src="https://github.com/amats6655/MineRedmine/assets/29190747/63e92cae-60bc-4022-9912-9f29b3b642ae" width=40%>
<img src="https://github.com/amats6655/MineRedmine/assets/29190747/fbcc275c-0a09-4834-a2ec-85f32057e74c" width=40%>






