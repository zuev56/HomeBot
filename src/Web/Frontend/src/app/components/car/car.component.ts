import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-car',
  templateUrl: './car.component.html',
  styleUrls: ['./car.component.scss']
})

// export означает, что все переменные класса будут экспортированы в HTML
// implements OnInit означает, что класс должен реализовывать метод OnInit (ngOnInit)
export class CarComponent implements OnInit {

  name: string;
  speed: number;
  model: string;
  colors: Colors;
  options: string[];
  editMode: boolean = false;
  test1: any = '11.11.2011';

  // Конструктор вызывается в первую очередь. Можно передавать в него параметры
  constructor() {
     this.name = '';
    this.speed = 0;
    this.model = '';
    this.colors = {
      car: '',
      salon: '',
      wheels: ''
    };
    this.options = [];
  }

  // Вызывается при создании, но после конструктора, можно сделать инициализацию переменных
  ngOnInit(): void {
    this.selectCar('Audi')
  }

  addOption(option: string): boolean {
    // Добавление в начало массива
    this.options.unshift(option);
    return false;
  }

  deleteOption(index: number): void {
    console.log(index + '. ' + this.options[index]);
    this.options.splice(index, 1);
  }

  showEditForm(): void {
    this.editMode = !this.editMode;
  }

  selectCar(carName: string) {
    if (carName == 'BMW') {
      this.name = 'BMW';
      this.speed = 280;
      this.model = 'M5';
      this.colors = {
        car: 'Синий',
        salon: 'Белый',
        wheels: 'Серебристый'
      };
      this.options = ["ABS", "Круиз контроль", "Магнитола"];
    } else if (carName == 'Audi'){
      this.name = 'Audi';
      this.speed = 235;
      this.model = 'RS8';
      this.colors = {
        car: 'Белый',
        salon: 'Чёрный',
        wheels: 'Серебристый'
      };
      this.options = ["ABS", "Глонасс", "Магнитола"];
    } else if (carName == 'Mercedes'){
      this.name = 'Mercedes';
      this.speed = 220;
      this.model = 'E400';
      this.colors = {
        car: 'Чёрный',
        salon: 'Коричневый',
        wheels: 'Серебристый'
      };
      this.options = ["ABS", "Глонасс", "Магнитола"];
    }
  }
}

interface Colors {
  car:string,
  salon:string,
  wheels:string
}